// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Apps;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using IEmailTemplateStore = Notifo.Domain.ChannelTemplates.IChannelTemplateStore<Notifo.Domain.Channels.Email.EmailTemplate>;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.Email.EmailJob>;

namespace Notifo.Domain.Channels.Email;

public sealed class EmailChannel : ICommunicationChannel, IScheduleHandler<EmailJob>
{
    private const string EmailAddress = nameof(EmailAddress);
    private readonly IAppStore appStore;
    private readonly IEmailFormatter emailFormatter;
    private readonly IEmailTemplateStore emailTemplateStore;
    private readonly IIntegrationManager integrationManager;
    private readonly ILogger<EmailChannel> log;
    private readonly ILogStore logStore;
    private readonly IUserNotificationQueue userNotificationQueue;
    private readonly IUserNotificationStore userNotificationStore;
    private readonly IUserStore userStore;

    public string Name => Providers.Email;

    public EmailChannel(
        IAppStore appStore,
        IEmailFormatter emailFormatter,
        IEmailTemplateStore emailTemplateStore,
        IIntegrationManager integrationManager,
        ILogger<EmailChannel> log,
        ILogStore logStore,
        IUserNotificationQueue userNotificationQueue,
        IUserNotificationStore userNotificationStore,
        IUserStore userStore)
    {
        this.appStore = appStore;
        this.emailFormatter = emailFormatter;
        this.emailTemplateStore = emailTemplateStore;
        this.log = log;
        this.logStore = logStore;
        this.integrationManager = integrationManager;
        this.userNotificationQueue = userNotificationQueue;
        this.userNotificationStore = userNotificationStore;
        this.userStore = userStore;
    }

    public IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context)
    {
        if (notification.Silent || string.IsNullOrEmpty(context.User.EmailAddress))
        {
            yield break;
        }

        if (!integrationManager.HasIntegration<IEmailSender>(context.App))
        {
            yield break;
        }

        yield return new SendConfiguration
        {
            [EmailAddress] = context.User.EmailAddress
        };
    }

    public async Task SendAsync(UserNotification notification, ChannelContext context,
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

            await userNotificationQueue.ScheduleGroupedAsync(
                job.ScheduleKey,
                job,
                job.SendDelay,
                false, ct);
        }
    }

    public async Task<bool> HandleAsync(List<EmailJob> jobs, bool isLastAttempt,
        CancellationToken ct)
    {
        var activityLinks = jobs.SelectMany(x => x.Notification.Links());
        var activityContext = Activity.Current?.Context ?? default;

        using (Telemetry.Activities.StartActivity("EmailChannel/Handle", ActivityKind.Internal, activityContext, links: activityLinks))
        {
            var unhandledJobs = new List<EmailJob>();

            foreach (var job in jobs)
            {
                if (await userNotificationStore.IsHandledAsync(job, this, ct))
                {
                    await UpdateAsync(job, DeliveryResult.Skipped());
                }
                else
                {
                    unhandledJobs.Add(job);
                }
            }

            if (unhandledJobs.Any())
            {
                await SendJobsAsync(unhandledJobs, ct);
            }

            return true;
        }
    }

    public Task HandleExceptionAsync(List<EmailJob> jobs, Exception ex)
    {
        return UpdateAsync(jobs, DeliveryResult.Failed());
    }

    public async Task SendJobsAsync(List<EmailJob> jobs,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("Send"))
        {
            var first = jobs[0];

            var commonEmail = first.EmailAddress;
            var commonApp = first.Notification.AppId;
            var commonUser = first.Notification.UserId;

            await UpdateAsync(jobs, DeliveryResult.Attempt);

            var app = await appStore.GetCachedAsync(first.Notification.AppId, ct);

            if (app == null)
            {
                log.LogWarning("Cannot send email: App not found.");

                await UpdateAsync(jobs, DeliveryResult.Handled);
                return;
            }

            try
            {
                var user = await userStore.GetCachedAsync(commonApp, commonUser, ct);

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

                var integrations = integrationManager.Resolve<IEmailSender>(app, first.Notification).ToList();

                if (integrations.Count == 0)
                {
                    await SkipAsync(jobs, LogMessage.Integration_Removed(Name));
                    return;
                }

                var (skip, message) = await BuildMessageAsync(jobs, app, user, ct);

                if (skip != null)
                {
                    await SkipAsync(jobs, skip.Value);
                    return;
                }

                var result = await SendCoreAsync(message!, first.Notification.AppId, integrations, ct);

                if (result.Status > DeliveryStatus.Attempt)
                {
                    await UpdateAsync(jobs, result);
                }
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(app.Id, LogMessage.General_Exception(Name, ex));
                throw;
            }
        }
    }

    private async Task<DeliveryResult> SendCoreAsync(EmailMessage message, string appId, List<ResolvedIntegration<IEmailSender>> integrations,
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
                await logStore.LogAsync(appId, LogMessage.General_Exception(Name, ex));

                // We only expose details of domain exceptions.
                lastResult = DeliveryResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                await logStore.LogAsync(appId, LogMessage.General_InternalException(sender.Definition.Type, ex));

                if (sender == integrations[^1].System)
                {
                    throw;
                }

                lastResult = DeliveryResult.Failed();
            }
        }

        return lastResult;
    }

    private Task UpdateAsync(EmailJob notification, DeliveryResult result)
    {
        return userNotificationStore.TrackAsync(notification.AsTrackingKey(Name), result);
    }

    private async Task SkipAsync(List<EmailJob> jobs, LogMessage message)
    {
        await logStore.LogAsync(jobs[0].Notification.AppId, message);

        await UpdateAsync(jobs, DeliveryResult.Skipped(message.Reason));
    }

    private async Task UpdateAsync(List<EmailJob> jobs, DeliveryResult result)
    {
        foreach (var job in jobs)
        {
            await UpdateAsync(job, result);
        }
    }

    private async Task<(LogMessage? Skip, EmailMessage?)> BuildMessageAsync(List<EmailJob> jobs, App app, User user,
        CancellationToken ct)
    {
        var firstJob = jobs[0];

        var (skip, template) = await GetTemplateAsync(
            firstJob.Notification.AppId,
            firstJob.Notification.UserLanguage,
            firstJob.EmailTemplate,
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

                    await logStore.LogAsync(appId, message);
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
