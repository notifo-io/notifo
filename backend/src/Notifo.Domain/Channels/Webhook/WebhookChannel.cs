// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Webhook;

public sealed class WebhookChannel : SchedulingChannelBase<WebhookJob, WebhookChannel>
{
    private const string IntegrationId = nameof(IntegrationId);

    public override string Name => Providers.Webhook;

    public override bool IsSystem => true;

    public WebhookChannel(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context)
    {
        var integrations = IntegrationManager.Resolve<IWebhookSender>(context.App, notification);

        foreach (var (id, _, _) in integrations)
        {
            yield return new SendConfiguration
            {
                [IntegrationId] = id
            };
        }
    }

    public override async Task SendAsync(UserNotification notification, ChannelContext context,
        CancellationToken ct)
    {
        if (!context.Configuration.TryGetValue(IntegrationId, out var integrationId))
        {
            // Old configuration without an integration id.
            return;
        }

        using (Telemetry.Activities.StartActivity("SmsChannel/SendAsync"))
        {
            var job = new WebhookJob(notification, context, integrationId);

            // Do not use scheduling when the notification is an update.
            if (job.IsUpdate)
            {
                await Scheduler.ScheduleAsync(
                    job.ScheduleKey,
                    job,
                    default(Instant),
                    false, ct);
            }
            else
            {
                await Scheduler.ScheduleAsync(
                    job.ScheduleKey,
                    job,
                    job.SendDelay,
                    false, ct);
            }
        }
    }

    protected override async Task SendJobsAsync(List<WebhookJob> jobs,
        CancellationToken ct)
    {
        // The schedule key is computed in a way that does not allow grouping. Therefore we have only one job.
        var job = jobs[0];

        using (Telemetry.Activities.StartActivity("Send"))
        {
            var app = await AppStore.GetCachedAsync(job.Notification.AppId, ct);

            if (app == null)
            {
                Log.LogWarning("Cannot send webhook: App not found.");

                await UpdateAsync(job, DeliveryResult.Handled);
                return;
            }

            var integration = IntegrationManager.Resolve<IWebhookSender>(job.IntegrationId, app);

            if (integration == default)
            {
                await SkipAsync(job, LogMessage.Integration_Removed(Name));
                return;
            }

            try
            {
                await UpdateAsync(job, DeliveryResult.Attempt);

                var result = await SendCoreAsync(job, integration, ct);

                if (result.Status > DeliveryStatus.Attempt)
                {
                    await UpdateAsync(job, result);
                }
            }
            catch (Exception ex)
            {
                await LogStore.LogAsync(job.Notification.AppId, LogMessage.General_InternalException(Name, ex));
                throw;
            }
        }
    }

    private Task<DeliveryResult> SendCoreAsync(WebhookJob job, ResolvedIntegration<IWebhookSender> integration,
        CancellationToken ct)
    {
        var message = new WebhookMessage
        {
            Payload = job.Notification
        };

        return integration.System.SendAsync(integration.Context, message.Enrich(job, Name), ct);
    }
}
