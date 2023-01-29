// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Mediator;
using Notifo.Infrastructure.Scheduling;
using IUserNotificationQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.Channels.MobilePush.MobilePushJob>;

namespace Notifo.Domain.Channels.MobilePush;

public sealed class MobilePushChannel : ICommunicationChannel, IScheduleHandler<MobilePushJob>
{
    private const string Token = nameof(Token);
    private const string DeviceType = nameof(DeviceType);
    private const string DeviceIdentifier = nameof(DeviceIdentifier);
    private readonly IAppStore appStore;
    private readonly IClock clock;
    private readonly IIntegrationManager integrationManager;
    private readonly IMediator mediator;
    private readonly ILogger<MobilePushChannel> log;
    private readonly ILogStore logStore;
    private readonly IUserNotificationQueue userNotificationQueue;
    private readonly IUserNotificationStore userNotificationStore;

    public string Name => Providers.MobilePush;

    public MobilePushChannel(ILogger<MobilePushChannel> log, ILogStore logStore,
        IAppStore appStore,
        IIntegrationManager integrationManager,
        IMediator mediator,
        IUserNotificationQueue userNotificationQueue,
        IUserNotificationStore userNotificationStore,
        IClock clock)
    {
        this.appStore = appStore;
        this.log = log;
        this.logStore = logStore;
        this.integrationManager = integrationManager;
        this.mediator = mediator;
        this.userNotificationQueue = userNotificationQueue;
        this.userNotificationStore = userNotificationStore;
        this.clock = clock;
    }

    public IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context)
    {
        if (!integrationManager.HasIntegration<IMobilePushSender>(context.App))
        {
            yield break;
        }

        foreach (var token in context.User.MobilePushTokens)
        {
            if (!string.IsNullOrWhiteSpace(token.Token))
            {
                yield return new SendConfiguration
                {
                    [Token] = token.Token,
                    [DeviceIdentifier] = token.DeviceIdentifier ?? string.Empty,
                    [DeviceType] = token.DeviceType.ToString()
                };
            }
        }
    }

    public async Task HandleSeenAsync(UserNotification notification, ChannelContext context)
    {
        using (Telemetry.Activities.StartActivity("MobilePushChannel/HandleSeenAsync"))
        {
            if (context.Configuration == null || context.Configuration.TryGetValue(Token, out var mobileToken))
            {
                // The configuration has no token.
                return;
            }

            var userToken = context.User.MobilePushTokens.FirstOrDefault(x => x.Token == mobileToken && x.DeviceType == MobileDeviceType.iOS);

            if (userToken == null)
            {
                // The token is not for iOS or has been removed.
                return;
            }

            await TryWakeupAsync(notification, context, userToken, default);
        }
    }

    public async Task SendAsync(UserNotification notification, ChannelContext context,
        CancellationToken ct)
    {
        if (!context.Configuration.TryGetValue(Token, out var tokenString))
        {
            // Old configuration without a mobile push token.
            return;
        }

        using (Telemetry.Activities.StartActivity("MobilePushChannel/SendAsync"))
        {
            var token = context.User.MobilePushTokens.SingleOrDefault(x => x.Token == tokenString);

            if (token == null)
            {
                // Token has been deleted in the meantime.
                return;
            }

            if (token.DeviceType == MobileDeviceType.iOS)
            {
                await TryWakeupAsync(notification, context, token, ct);
            }

            var job = new MobilePushJob(notification, context, token);

            if (context.IsUpdate)
            {
                await userNotificationQueue.ScheduleAsync(
                    job.ScheduleKey,
                    job,
                    default(Instant),
                    false, ct);
            }
            else
            {
                await userNotificationQueue.ScheduleAsync(
                    job.ScheduleKey,
                    job,
                    job.SendDelay,
                    false, ct);
            }
        }
    }

    private async Task TryWakeupAsync(UserNotification notification, ChannelContext context, MobilePushToken token,
        CancellationToken ct)
    {
        var wakeupTime = token.GetNextWakeupTime(clock);

        if (wakeupTime == null)
        {
            return;
        }

        var dummyNotification = new UserNotification
        {
            AppId = notification.AppId,
            UserId = notification.UserId,
            UserLanguage = notification.UserLanguage
        };

        var wakeupJob = new MobilePushJob(dummyNotification, context, token);

        await userNotificationQueue.ScheduleAsync(
            wakeupJob.ScheduleKey,
            wakeupJob,
            wakeupTime.Value,
            false, ct);

        try
        {
            var command = new UpdateMobileWakeupTime
            {
                Token = token.Token
            };

            await mediator.SendAsync(command.With(notification.AppId, notification.UserId), ct);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Failed to wakeup device.");
        }
    }

    public async Task<bool> HandleAsync(MobilePushJob job, bool isLastAttempt,
        CancellationToken ct)
    {
        var activityLinks = job.Notification.Links();
        var activityContext = Activity.Current?.Context ?? default;

        using (Telemetry.Activities.StartActivity("MobilePushChannel/HandleAsync", ActivityKind.Internal, activityContext, links: activityLinks))
        {
            if (await userNotificationStore.IsHandledAsync(job, this, ct))
            {
                await UpdateAsync(job, DeliveryResult.Skipped());
            }
            else
            {
                await SendJobAsync(job, ct);
            }

            return true;
        }
    }

    public Task HandleExceptionAsync(MobilePushJob job, Exception ex)
    {
        return UpdateAsync(job, DeliveryResult.Failed());
    }

    private async Task SendJobAsync(MobilePushJob job,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("SendMobilePush"))
        {
            var notification = job.Notification;

            var app = await appStore.GetCachedAsync(notification.AppId, ct);

            if (app == null)
            {
                log.LogWarning("Cannot send mobile push: App not found.");

                await UpdateAsync(job, DeliveryResult.Handled);
                return;
            }

            try
            {
                await UpdateAsync(job, DeliveryResult.Attempt);

                var integrations = integrationManager.Resolve<IMobilePushSender>(app, notification).ToList();

                if (integrations.Count == 0)
                {
                    await SkipAsync(job, LogMessage.Integration_Removed(Name));
                    return;
                }

                var message = BuildMessage(job);

                var result = await SendCoreAsync(job, message, integrations, ct);

                if (result.Status > DeliveryStatus.Attempt)
                {
                    await UpdateAsync(job, result);
                }
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(job.Notification.AppId, LogMessage.General_Exception(Name, ex));
                throw;
            }
        }
    }

    private async Task<DeliveryResult> SendCoreAsync(MobilePushJob job, MobilePushMessage message, List<ResolvedIntegration<IMobilePushSender>> integrations,
        CancellationToken ct)
    {
        var lastResult = default(DeliveryResult);

        foreach (var (_, context, sender) in integrations)
        {
            try
            {
                lastResult = await sender.SendAsync(context, message, ct);

                if (lastResult.Status >= DeliveryStatus.Sent)
                {
                    return lastResult;
                }
            }
            catch (MobilePushTokenExpiredException)
            {
                await logStore.LogAsync(job.Notification.AppId, LogMessage.MobilePush_TokenInvalid(Name, job.Notification.UserId, job.Token));

                var command = new RemoveUserMobileToken
                {
                    Token = job.Token
                };

                await mediator.SendAsync(command.With(job.Notification.AppId, job.Notification.UserId), ct);
                break;
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(job.Notification.AppId, LogMessage.General_Exception(Name, ex));

                // We only expose details of domain exceptions.
                lastResult = DeliveryResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                await logStore.LogAsync(job.Notification.AppId, LogMessage.General_InternalException(Name, ex));

                if (sender == integrations[^1].System)
                {
                    throw;
                }

                lastResult = DeliveryResult.Failed();
            }
        }

        return lastResult;
    }

    private async Task SkipAsync(MobilePushJob job, LogMessage message)
    {
        await logStore.LogAsync(job.Notification.AppId, message);

        await UpdateAsync(job, DeliveryResult.Skipped(message.Reason));
    }

    private async Task UpdateAsync(MobilePushJob job, DeliveryResult result)
    {
        // We only track the initial publication.
        if (!job.IsUpdate)
        {
            await userNotificationStore.TrackAsync(job.AsTrackingKey(Name), result);
        }
    }

    private MobilePushMessage BuildMessage(MobilePushJob job)
    {
        var notification = job.Notification;

        var message = new MobilePushMessage
        {
            Subject = notification.Formatting.Subject,
            Body = notification.Formatting.Body,
            ConfirmText = notification.Formatting.ConfirmText,
            ConfirmUrl = notification.ComputeConfirmUrl(Providers.MobilePush, job.ConfigurationId),
            Data = notification.Data,
            DeviceToken = job.Token,
            DeviceType = job.DeviceType,
            ImageLarge = notification.Formatting.ImageLarge,
            ImageSmall = notification.Formatting.ImageSmall,
            LinkText = notification.Formatting.LinkText,
            LinkUrl = notification.Formatting.LinkUrl,
            TimeToLiveInSeconds = notification.TimeToLiveInSeconds,
            Wakeup = notification.Formatting == null
        };

        return message.Enrich(job, Name);
    }
}
