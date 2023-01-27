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
    private readonly IUserStore userStore;

    public string Name => Providers.MobilePush;

    public MobilePushChannel(ILogger<MobilePushChannel> log, ILogStore logStore,
        IAppStore appStore,
        IIntegrationManager integrationManager,
        IMediator mediator,
        IUserNotificationQueue userNotificationQueue,
        IUserNotificationStore userNotificationStore,
        IUserStore userStore,
        IClock clock)
    {
        this.appStore = appStore;
        this.log = log;
        this.logStore = logStore;
        this.integrationManager = integrationManager;
        this.mediator = mediator;
        this.userNotificationQueue = userNotificationQueue;
        this.userNotificationStore = userNotificationStore;
        this.userStore = userStore;
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
                    job.Delay,
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
            }.WithBaseProperties(notification.AppId, notification.UserId).WithTimestamp(wakeupTime.Value);

            await mediator.SendAsync(command, ct);
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
                await UpdateAsync(job, ProcessStatus.Skipped);
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
        return UpdateAsync(job, ProcessStatus.Failed);
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

                await UpdateAsync(job, ProcessStatus.Handled);
                return;
            }

            try
            {
                await UpdateAsync(job, ProcessStatus.Attempt);

                var senders = integrationManager.Resolve<IMobilePushSender>(app, notification).Select(x => x.Integration).ToList();

                if (senders.Count == 0)
                {
                    await SkipAsync(job, LogMessage.Integration_Removed(Name));
                    return;
                }

                await SendCoreAsync(job, app, senders, ct);

                await UpdateAsync(job, ProcessStatus.Handled);
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(app.Id, LogMessage.General_Exception(Name, ex));
                throw;
            }
        }
    }

    private async Task SendCoreAsync(MobilePushJob job, App app, List<IMobilePushSender> senders,
        CancellationToken ct)
    {
        var notification = job.Notification;

        foreach (var sender in senders)
        {
            try
            {
                var message = new MobilePushMessage
                {
                    Body = notification.Formatting.Body,
                    ConfirmText = notification.Formatting.ConfirmText,
                    ConfirmUrl = notification.ComputeConfirmUrl(Providers.MobilePush, job.ConfigurationId),
                    Data = notification.Data,
                    DeviceToken = job.Token,
                    DeviceType = job.DeviceType,
                    ImageLarge = notification.Formatting.ImageLarge,
                    ImageSmall = notification.Formatting.ImageSmall,
                    IsConfirmed = job.IsConfirmed,
                    LinkText = notification.Formatting.LinkText,
                    LinkUrl = notification.Formatting.LinkUrl,
                    NotificationId = job.Notification.Id,
                    Silent = notification.Silent,
                    Subject = notification.Formatting.Subject,
                    TimeToLiveInSeconds = notification.TimeToLiveInSeconds,
                    TrackDeliveredUrl = notification.ComputeTrackDeliveredUrl(Providers.MobilePush, job.ConfigurationId),
                    TrackingToken = new TrackingToken(job.Notification.Id, Providers.MobilePush, job.ConfigurationId).ToParsableString(),
                    TrackSeenUrl = notification.ComputeTrackSeenUrl(Providers.MobilePush, job.ConfigurationId),
                    Wakeup = notification.Formatting == null
                };

                await sender.SendAsync(message.Enrich(job), ct);
                return;
            }
            catch (MobilePushTokenExpiredException)
            {
                await logStore.LogAsync(app.Id, LogMessage.MobilePush_TokenInvalid(Name, job.Notification.UserId, job.Token));

                var command = new RemoveUserMobileToken
                {
                    Token = job.Token
                }.WithBaseProperties(notification.AppId, notification.UserId);

                await mediator.SendAsync(command, ct);
                break;
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(app.Id, LogMessage.General_Exception(Name, ex));

                if (sender == senders[^1])
                {
                    // Some integrations provide the actual result via webhook at a later point.
                    throw;
                }
            }
            catch (Exception)
            {
                if (sender == senders[^1])
                {
                    // Some integrations provide the actual result via webhook at a later point.
                    throw;
                }
            }
        }
    }

    private async Task UpdateAsync(MobilePushJob job, ProcessStatus status, string? reason = null)
    {
        // We only track the initial publication.
        if (!job.IsUpdate)
        {
            await userNotificationStore.TrackAsync(job.AsTrackingKey(Name), status, reason);
        }
    }

    private async Task SkipAsync(MobilePushJob job, LogMessage message)
    {
        await logStore.LogAsync(job.Notification.AppId, message);

        await UpdateAsync(job, ProcessStatus.Skipped, message.Reason);
    }
}
