// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Json;
using WebPush;

namespace Notifo.Domain.Channels.WebPush;

public sealed class WebPushChannel : SchedulingChannelBase<WebPushJob, WebPushChannel>, IWebPushService
{
    private const string Endpoint = nameof(Endpoint);
    private readonly WebPushClient webPushClient = new WebPushClient();
    private readonly IJsonSerializer serializer;

    public override string Name => Providers.WebPush;

    public string PublicKey { get; }

    public WebPushChannel(IServiceProvider serviceProvider,
        IJsonSerializer serializer, IOptions<WebPushOptions> options)
        : base(serviceProvider)
    {
        this.serializer = serializer;

        webPushClient.SetVapidDetails(
            options.Value.Subject,
            options.Value.VapidPublicKey,
            options.Value.VapidPrivateKey);

        PublicKey = options.Value.VapidPublicKey;
    }

    public override IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context)
    {
        if (notification.Silent)
        {
            yield break;
        }

        foreach (var subscription in context.User.WebPushSubscriptions)
        {
            if (!string.IsNullOrWhiteSpace(subscription.Endpoint) &&
                subscription.Keys.ContainsKey("p256dh") &&
                subscription.Keys.ContainsKey("auth"))
            {
                yield return new SendConfiguration
                {
                    [Endpoint] = subscription.Endpoint
                };
            }
        }
    }

    public override async Task SendAsync(UserNotification notification, ChannelContext context,
        CancellationToken ct)
    {
        if (!context.Configuration.TryGetValue(Endpoint, out var endpoint))
        {
            // Old configuration without a mobile push token.
            return;
        }

        using (Telemetry.Activities.StartActivity("WebPushChannel/SendAsync"))
        {
            var subscription = context.User.WebPushSubscriptions.FirstOrDefault(x => x.Endpoint == endpoint);

            if (subscription == null)
            {
                // Subscription has been removed in the meantime.
                return;
            }

            var job = new WebPushJob(notification, context, subscription, serializer);

            // Do not use scheduling when the notification is an update.
            if (context.IsUpdate)
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

    protected override async Task SendJobsAsync(List<WebPushJob> jobs,
        CancellationToken ct)
    {
        // The schedule key is computed in a way that does not allow grouping. Therefore we have only one job.
        var job = jobs[0];

        using (Telemetry.Activities.StartActivity("Send"))
        {
            try
            {
                if (!job.IsUpdate)
                {
                    await UpdateAsync(job, DeliveryResult.Attempt);
                }

                var result = await SendCoreAsync(job, ct);

                if (!job.IsUpdate)
                {
                    await UpdateAsync(job, result);
                }
            }
            catch (DomainException ex)
            {
                await LogStore.LogAsync(job.Notification.AppId, LogMessage.General_Exception(Name, ex));
                throw;
            }
        }
    }

    private async Task<DeliveryResult> SendCoreAsync(WebPushJob job,
        CancellationToken ct)
    {
        try
        {
            var pushSubscription = new PushSubscription(
                job.Subscription.Endpoint,
                job.Subscription.Keys["p256dh"],
                job.Subscription.Keys["auth"]);

            var json = job.Payload;

            await webPushClient.SendNotificationAsync(pushSubscription, json, cancellationToken: ct);
            return DeliveryResult.Handled;
        }
        catch (WebPushException ex) when (ex.StatusCode == HttpStatusCode.Gone)
        {
            // Use the same log message for the delivery result later.
            var logMessage = LogMessage.WebPush_TokenInvalid(Name, job.Notification.UserId, job.Subscription.Endpoint);

            await LogStore.LogAsync(job.Notification.AppId, logMessage);
            await RemoveTokenAsync(job);

            return DeliveryResult.Failed(logMessage.Reason);
        }
        catch (WebPushException ex)
        {
            throw new DomainException(ex.Message);
        }
    }

    private async Task RemoveTokenAsync(WebPushJob job)
    {
        try
        {
            var command = new RemoveUserWebPushSubscription
            {
                Endpoint = job.Subscription.Endpoint
            };

            await Mediator.SendAsync(command.With(job.Notification.AppId, job.Notification.UserId));
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "Failed to remove token.");
        }
    }
}
