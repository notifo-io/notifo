// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.MobilePush;

public sealed class MobilePushChannel : SchedulingChannelBase<MobilePushJob, MobilePushChannel>
{
    private const string Token = nameof(Token);
    private const string DeviceType = nameof(DeviceType);
    private const string DeviceIdentifier = nameof(DeviceIdentifier);

    public IClock Clock { get; } = SystemClock.Instance;

    public override string Name => Providers.MobilePush;

    public MobilePushChannel(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context)
    {
        if (!IntegrationManager.HasIntegration<IMobilePushSender>(context.App))
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

    public override async Task HandleSeenAsync(UserNotification notification, ChannelContext context)
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

    public override async Task SendAsync(UserNotification notification, ChannelContext context,
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

    private async Task TryWakeupAsync(UserNotification notification, ChannelContext context, MobilePushToken token,
        CancellationToken ct)
    {
        var wakeupTime = token.GetNextWakeupTime(Clock);

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

        await Scheduler.ScheduleAsync(
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

            await Mediator.SendAsync(command.With(notification.AppId, notification.UserId), ct);
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "Failed to wakeup device.");
        }
    }

    protected override async Task SendJobsAsync(List<MobilePushJob> jobs,
        CancellationToken ct)
    {
        // The schedule key is computed in a way that does not allow grouping. Therefore we have only one job.
        var job = jobs[0];

        using (Telemetry.Activities.StartActivity("SendMobilePush"))
        {
            var notification = job.Notification;

            var app = await AppStore.GetCachedAsync(notification.AppId, ct);

            if (app == null)
            {
                Log.LogWarning("Cannot send mobile push: App not found.");

                await UpdateAsync(job, DeliveryResult.Handled);
                return;
            }

            var integrations = IntegrationManager.Resolve<IMobilePushSender>(app, notification).ToList();

            if (integrations.Count == 0)
            {
                await SkipAsync(job, LogMessage.Integration_Removed(Name));
                return;
            }

            await UpdateAsync(job, DeliveryResult.Attempt);
            try
            {
                var message = BuildMessage(job);

                var result = await SendCoreAsync(job, message, integrations, ct);

                if (result.Status > DeliveryStatus.Attempt)
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
                // Use the same log message for the delivery result later.
                var logMessage = LogMessage.MobilePush_TokenInvalid(Name, job.Notification.UserId, job.DeviceToken);

                await LogStore.LogAsync(job.Notification.AppId, logMessage);
                await RemoveTokenAsync(job);

                // If the token is gone, it is gone for all providers, so there is no need to try another provider.
                return DeliveryResult.Failed(logMessage.Reason);
            }
            catch (DomainException ex)
            {
                await LogStore.LogAsync(job.Notification.AppId, LogMessage.General_Exception(Name, ex));

                // We only expose details of domain exceptions.
                lastResult = DeliveryResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                await LogStore.LogAsync(job.Notification.AppId, LogMessage.General_InternalException(Name, ex));

                if (sender == integrations[^1].System)
                {
                    throw;
                }

                lastResult = DeliveryResult.Failed();
            }
        }

        return lastResult;
    }

    private async Task RemoveTokenAsync(MobilePushJob job)
    {
        try
        {
            var command = new RemoveUserMobileToken
            {
                Token = job.DeviceToken
            };

            await Mediator.SendAsync(command.With(job.Notification.AppId, job.Notification.UserId));
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "Failed to remove token.");
        }
    }

    private MobilePushMessage BuildMessage(MobilePushJob job)
    {
        var notification = job.Notification;

        var message = new MobilePushMessage
        {
            Subject = notification.Formatting?.Subject,
            Body = notification.Formatting?.Body,
            ConfirmLink = notification.Formatting?.ConfirmLink,
            ConfirmText = notification.Formatting?.ConfirmText,
            ConfirmUrl = notification.ComputeConfirmUrl(Providers.MobilePush, job.ConfigurationId),
            Data = notification.Data,
            DeviceToken = job.DeviceToken,
            DeviceType = job.DeviceType,
            ImageLarge = notification.Formatting?.ImageLarge,
            ImageSmall = notification.Formatting?.ImageSmall,
            LinkText = notification.Formatting?.LinkText,
            LinkUrl = notification.Formatting?.LinkUrl,
            TimeToLiveInSeconds = notification.TimeToLiveInSeconds,
            Wakeup = notification.Formatting == null
        };

        return message.Enrich(job, Name);
    }
}
