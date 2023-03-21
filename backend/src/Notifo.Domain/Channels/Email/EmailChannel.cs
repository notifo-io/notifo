// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Notifo.Domain.Apps;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using IEmailTemplateStore = Notifo.Domain.ChannelTemplates.IChannelTemplateStore<Notifo.Domain.Channels.Email.EmailTemplate>;

namespace Notifo.Domain.Channels.Email;

public sealed class EmailChannel : SchedulingChannelBase<EmailJob, EmailChannel>
{
    private const string EmailAddress = nameof(EmailAddress);
    private readonly IEmailFormatter emailFormatter;
    private readonly IEmailTemplateStore emailTemplateStore;

    public override string Name => Providers.Email;

    public EmailChannel(IServiceProvider serviceProvider,
        IEmailFormatter emailFormatter,
        IEmailTemplateStore emailTemplateStore)
        : base(serviceProvider)
    {
        this.emailFormatter = emailFormatter;
        this.emailTemplateStore = emailTemplateStore;
    }

    public override IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context)
    {
        if (notification.Silent || string.IsNullOrEmpty(context.User.EmailAddress))
        {
            yield break;
        }

        if (!IntegrationManager.HasIntegration<IEmailSender>(context.App))
        {
            yield break;
        }

        yield return new SendConfiguration
        {
            [EmailAddress] = context.User.EmailAddress
        };
    }

    public override async Task SendAsync(UserNotification notification, ChannelContext context,
        CancellationToken ct)
    {
        if (context.IsUpdate)
        {
            return;
        }

        if (!context.Configuration.TryGetValue(EmailAddress, out var email))
        {
            // Old configuration without a email address.
            return;
        }

        using (Telemetry.Activities.StartActivity("EmailChannel/SendAsync"))
        {
            var job = new EmailJob(notification, context, email);

            await Scheduler.ScheduleGroupedAsync(
                job.ScheduleKey,
                job,
                job.SendDelay,
                false, ct);
        }
    }

    protected override async Task SendJobsAsync(List<EmailJob> jobs,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("Send"))
        {
            var lastJob = jobs[^1];

            var commonEmail = lastJob.EmailAddress;
            var commonApp = lastJob.Notification.AppId;
            var commonUser = lastJob.Notification.UserId;

            var app = await AppStore.GetCachedAsync(lastJob.Notification.AppId, ct);

            if (app == null)
            {
                Log.LogWarning("Cannot send email: App not found.");

                await UpdateAsync(jobs, DeliveryResult.Handled);
                return;
            }

            var user = await UserStore.GetCachedAsync(commonApp, commonUser, ct);

            if (user == null)
            {
                await SkipAsync(jobs, LogMessage.User_Deleted(Name, commonUser));
                return;
            }

            if (string.IsNullOrWhiteSpace(user.EmailAddress))
            {
                await SkipAsync(jobs, LogMessage.User_EmailRemoved(Name, commonUser));
                return;
            }

            var integrations = IntegrationManager.Resolve<IEmailSender>(app, lastJob.Notification).ToList();

            if (integrations.Count == 0)
            {
                await SkipAsync(jobs, LogMessage.Integration_Removed(Name));
                return;
            }

            await UpdateAsync(jobs, DeliveryResult.Attempt);
            try
            {
                var (skip, message) = await BuildMessageAsync(jobs, app, user, ct);

                if (skip != null)
                {
                    await SkipAsync(jobs, skip.Value);
                    return;
                }

                var result = await SendCoreAsync(lastJob.Notification.AppId, message!, integrations, ct);

                if (result.Status > DeliveryStatus.Attempt)
                {
                    await UpdateAsync(jobs, result);
                }
            }
            catch (DomainException ex)
            {
                await LogStore.LogAsync(commonApp, LogMessage.General_Exception(Name, ex));
                throw;
            }
        }
    }

    private async Task<DeliveryResult> SendCoreAsync(string appId, EmailMessage message, List<ResolvedIntegration<IEmailSender>> integrations,
        CancellationToken ct)
    {
        var lastResult = default(DeliveryResult);

        foreach (var (_, context, sender) in integrations)
        {
            try
            {
                lastResult = await sender.SendAsync(context, message, ct);

                // We only sent notifications over the first successful integration.
                if (lastResult.Status >= DeliveryStatus.Sent)
                {
                    break;
                }
            }
            catch (DomainException ex)
            {
                await LogStore.LogAsync(appId, LogMessage.General_Exception(Name, ex));

                // We only expose details of domain exceptions.
                lastResult = DeliveryResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                await LogStore.LogAsync(appId, LogMessage.General_InternalException(sender.Definition.Type, ex));

                if (sender == integrations[^1].System)
                {
                    throw;
                }

                lastResult = DeliveryResult.Failed();
            }
        }

        return lastResult;
    }

    private async Task<(LogMessage? Skip, EmailMessage?)> BuildMessageAsync(List<EmailJob> jobs, App app, User user,
        CancellationToken ct)
    {
        var firstJob = jobs[0];

        var (skip, template) = await GetTemplateAsync(
            firstJob.Notification.AppId,
            firstJob.Notification.UserLanguage,
            firstJob.Template,
            ct);

        if (skip != default)
        {
            return (skip, null);
        }

        var (message, errors) = await emailFormatter.FormatAsync(template!, jobs, app, user, false, ct);

        if (errors?.Count > 0 || message == null)
        {
            errors ??= new List<EmailFormattingError>();

            if (errors.Count == 0)
            {
                errors.Add(new EmailFormattingError(EmailTemplateType.General, Texts.Email_UnknownErrror));
            }

            throw new EmailFormattingException(errors);
        }

        return (default, message);
    }

    private async Task<(LogMessage? Skip, EmailTemplate?)> GetTemplateAsync(
        string appId,
        string language,
        string? name,
        CancellationToken ct)
    {
        var (status, template) = await emailTemplateStore.GetBestAsync(appId, name, language, ct);

        switch (status)
        {
            case TemplateResolveStatus.ResolvedWithFallback:
                {
                    var message = LogMessage.ChannelTemplate_ResolvedWithFallback(Name, name);

                    await LogStore.LogAsync(appId, message);
                    break;
                }

            case TemplateResolveStatus.NotFound:
                {
                    var message = LogMessage.ChannelTemplate_NotFound(Name, name);

                    return (message, null);
                }

            case TemplateResolveStatus.LanguageNotFound:
                {
                    var message = LogMessage.ChannelTemplate_LanguageNotFound(Name, language, name);

                    return (message, null);
                }
        }

        return (null, template);
    }
}
