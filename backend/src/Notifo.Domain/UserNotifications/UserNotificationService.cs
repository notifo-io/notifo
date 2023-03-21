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
using Notifo.Domain.Channels;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserEvents;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using Squidex.Messaging;
using IUserEventQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.UserEvents.UserEventMessage>;

namespace Notifo.Domain.UserNotifications;

public sealed class UserNotificationService : IUserNotificationService, IScheduleHandler<UserEventMessage>, IMessageHandler<ConfirmMessage>
{
    private readonly Dictionary<string, ICommunicationChannel> channels;
    private readonly IAppStore appStore;
    private readonly ILogger<UserNotificationService> log;
    private readonly ILogStore logStore;
    private readonly IMessageBus messageBus;
    private readonly IUserEventQueue userEventQueue;
    private readonly IUserNotificationFactory userNotificationFactory;
    private readonly IUserNotificationStore userNotificationsStore;
    private readonly IUserStore userStore;
    private readonly IClock clock;

    public UserNotificationService(
        IEnumerable<ICommunicationChannel> channels,
        IAppStore appStore,
        ILogger<UserNotificationService> log,
        ILogStore logStore,
        IMessageBus messageBus,
        IUserEventQueue userEventQueue,
        IUserNotificationFactory userNotificationFactory,
        IUserNotificationStore userNotificationsStore,
        IUserStore userStore,
        IClock clock)
    {
        this.appStore = appStore;
        this.channels = channels.ToDictionary(x => x.Name);
        this.log = log;
        this.logStore = logStore;
        this.messageBus = messageBus;
        this.userEventQueue = userEventQueue;
        this.userNotificationFactory = userNotificationFactory;
        this.userNotificationsStore = userNotificationsStore;
        this.userStore = userStore;
        this.clock = clock;
    }

    public Task HandleExceptionAsync(List<UserEventMessage> jobs, Exception exception)
    {
        return Task.CompletedTask;
    }

    public async Task<bool> HandleAsync(List<UserEventMessage> jobs, bool isLastAttempt,
        CancellationToken ct)
    {
        await DistributeScheduledAsync(jobs[^1], jobs.SkipLast(1), isLastAttempt);
        return true;
    }

    public async Task DistributeAsync(UserEventMessage userEvent)
    {
        using (Telemetry.Activities.StartActivity("HandleUserEvent"))
        {
            var user = await userStore.GetCachedAsync(userEvent.AppId, userEvent.UserId);

            if (user == null)
            {
                await MarkFailedAsync(userEvent, LogMessage.User_Deleted("System", userEvent.UserId));
                return;
            }

            // The scheduling from the event has preference over the user scheduling.
            userEvent.Scheduling = Scheduling.Merged(user.Scheduling, userEvent.Scheduling);

            var scheduleKey = ScheduleKey(userEvent);
            var scheduleTime = Scheduling.CalculateScheduleTime(userEvent.Scheduling, clock, user.PreferredTimezone);

            await userEventQueue.ScheduleGroupedAsync(scheduleKey, userEvent, scheduleTime, true);
        }
    }

    public async Task DistributeScheduledAsync(UserEventMessage userEvent, IEnumerable<UserEventMessage> children, bool isLastAttempt)
    {
        var activityLinks = userEvent.Links();
        var activityContext = Activity.Current?.Context ?? default;

        using (var activity = Telemetry.Activities.StartActivity("DistributeUserEventScheduled", ActivityKind.Internal, activityContext, links: activityLinks))
        {
            await userNotificationsStore.TrackAsync(userEvent, DeliveryResult.Attempt);

            try
            {
                var context = await BuildContextAsync(userEvent.AppId, userEvent.UserId, false, default);

                if (context == null)
                {
                    return;
                }

                var notification = await CreateUserNotificationAsync(userEvent, children, context);

                if (notification == null)
                {
                    return;
                }

                // Assign the notification activity, so that we can continue with that when the handle the event.
                notification.UserNotificationActivity = activity?.Context ?? default;

                try
                {
                    await userNotificationsStore.InsertAsync(notification);
                }
                catch (UniqueConstraintException)
                {
                    await logStore.LogAsync(userEvent.AppId, LogMessage.Notification_AlreadyProcessed("System"));
                    return;
                }

                foreach (var channel in channels.Values)
                {
                    if (!notification.Channels.TryGetValue(channel.Name, out var notificationChannel))
                    {
                        continue;
                    }

                    context.Setting = notificationChannel.Setting;

                    foreach (var (id, status) in notificationChannel.Status)
                    {
                        context.ConfigurationId = id;
                        context.Configuration = status.Configuration;

                        await channel.SendAsync(notification, context, default);
                    }
                }

                log.LogInformation("Processed user event for app {appId} with ID {id} to topic {topic}.",
                    userEvent.AppId,
                    userEvent.EventId,
                    userEvent.Topic);
            }
            catch (Exception ex)
            {
                if (isLastAttempt)
                {
                    await userNotificationsStore.TrackAsync(userEvent, DeliveryResult.Failed());
                }

                log.LogError(ex, "Failed to process user event for app {appId} with ID {id} to topic {topic}.",
                    userEvent.AppId,
                    userEvent.EventId,
                    userEvent.Topic);
                throw;
            }
        }
    }

    private async Task<UserNotification?> CreateUserNotificationAsync(UserEventMessage userEvent, IEnumerable<UserEventMessage> children, ChannelContext context)
    {
        using (Telemetry.Activities.StartActivity("CreateUserNotification"))
        {
            var notification = userNotificationFactory.Create(context.App, context.User, userEvent, children);

            if (notification == null)
            {
                await logStore.LogAsync(userEvent.AppId, LogMessage.Notification_NoSubject("System"));
                return null;
            }

            foreach (var channel in channels.Values)
            {
                if (channel.IsSystem && !string.IsNullOrWhiteSpace(channel.Name))
                {
                    if (!notification.Channels.TryGetValue(channel.Name, out var channelInfo))
                    {
                        channelInfo = new UserNotificationChannel
                        {
                            Setting = new ChannelSetting
                            {
                                Send = ChannelSend.Send
                            }
                        };

                        notification.Channels[channel.Name] = channelInfo;
                    }
                }

                if (notification.Channels.TryGetValue(channel.Name, out var channelConfig) && channelConfig.Setting.Send == ChannelSend.Send)
                {
                    context.Configuration = null!;
                    context.ConfigurationId = default;
                    context.Setting = channelConfig.Setting;

                    var configurations = channel.GetConfigurations(notification, context);
                    var configurationSet = false;

                    foreach (var configuration in configurations.NotNull())
                    {
                        var status = new ChannelSendInfo
                        {
                            Configuration = configuration
                        };

                        channelConfig.Status[Guid.NewGuid()] = status;

                        // Use this flag to avoid allocations.
                        configurationSet = true;
                    }

                    if (!configurationSet && channelConfig.Setting.Required == ChannelRequired.Required)
                    {
                        var message = LogMessage.Event_ChannelRequired(channel.Name);

                        // Write to app and to user log store, but only log to standard out once.
                        await logStore.LogAsync(notification.AppId, message, true);
                        await logStore.LogAsync(notification.AppId, notification.UserId, message);
                    }
                }
            }

            return notification;
        }
    }

    public async Task HandleAsync(ConfirmMessage message,
        CancellationToken ct)
    {
        var notifications = await userNotificationsStore.TrackConfirmedAsync(new[] { message.Token }, ct);
#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections
        var notification = notifications.FirstOrDefault().Item1;
#pragma warning restore CA1826 // Do not use Enumerable methods on indexable collections

        if (notification == null || !notification.Channels.Any())
        {
            return;
        }

        try
        {
            var context = await BuildContextAsync(notification.AppId, notification.UserId, true, ct);

            if (context == null)
            {
                return;
            }

            foreach (var channel in channels.Values)
            {
                if (!notification.Channels.TryGetValue(channel.Name, out var channelInfo))
                {
                    continue;
                }

                context.Setting = channelInfo.Setting;

                foreach (var (id, status) in channelInfo.Status)
                {
                    context.ConfigurationId = id;
                    context.Configuration = status.Configuration;

                    await channel.SendAsync(notification, context, ct);
                }
            }
        }
        catch (DomainException ex)
        {
            await logStore.LogAsync(notification.AppId, LogMessage.General_Exception("System", ex));
            throw;
        }
    }

    public async Task TrackDeliveredAsync(params TrackingToken[] tokens)
    {
        Guard.NotNull(tokens);

        await userNotificationsStore.TrackDeliveredAsync(tokens);
    }

    public async Task TrackConfirmedAsync(params TrackingToken[] tokens)
    {
        Guard.NotNull(tokens);

        foreach (var token in tokens.Where(x => x.IsValid))
        {
            await messageBus.PublishAsync(new ConfirmMessage { Token = token }, token.ToString());
        }
    }

    public async Task TrackSeenAsync(params TrackingToken[] tokens)
    {
        Guard.NotNull(tokens);

        var notifications = await userNotificationsStore.TrackSeenAsync(tokens);

        var handled = new HashSet<(Guid NotificationID, string Channel, Guid ConfigurationId)>();

        foreach (var (notification, updated) in notifications)
        {
            if (!updated)
            {
                // If the notification has not been updated this tracking has happened before and we can just skip it.
                continue;
            }

            var context = await BuildContextAsync(notification.AppId, notification.UserId, true, default);

            if (context == null)
            {
                continue;
            }

            foreach (var token in tokens.Where(x => x.UserNotificationId == notification.Id))
            {
                // Ensure that we only call the channel once for each notification and configuration.
                if (string.IsNullOrWhiteSpace(token.Channel) || !handled.Add((notification.Id, token.Channel, token.ConfigurationId)))
                {
                    continue;
                }

                if (!notification.Channels.TryGetValue(token.Channel, out var channelInfo))
                {
                    continue;
                }

                if (!channels.TryGetValue(token.Channel, out var channel))
                {
                    continue;
                }

                channelInfo.Status.TryGetValue(token.ConfigurationId, out var status);

                context.ConfigurationId = token.ConfigurationId;
                context.Configuration = status?.Configuration!;
                context.Setting = channelInfo.Setting;

                await channel.HandleSeenAsync(notification, context);
            }
        }
    }

    private async Task MarkFailedAsync(UserEventMessage userEvent, LogMessage message)
    {
        await logStore.LogAsync(userEvent.AppId!, message);

        await userNotificationsStore.TrackAsync(TrackingKey.ForUserEvent(userEvent), DeliveryResult.Failed(message.Reason));
    }

    private async Task<ChannelContext?> BuildContextAsync(string appId, string userId, bool isUpdate,
        CancellationToken ct)
    {
        var app = await appStore.GetCachedAsync(appId, ct);

        if (app == null)
        {
            // Make no sense to log, because nobody would actually see it.
            return null;
        }

        var user = await userStore.GetCachedAsync(appId, userId, ct);

        if (user == null)
        {
            await logStore.LogAsync(appId, LogMessage.User_Deleted("System", userId));
            return null;
        }

        var context = new ChannelContext
        {
            App = app,
            AppId = appId,
            Configuration = default!,
            ConfigurationId = default,
            IsUpdate = isUpdate,
            Setting = default!,
            User = user,
            UserId = userId,
        };

        return context;
    }

    private static string ScheduleKey(UserEventMessage userEvent)
    {
        return $"{userEvent.AppId}_{userEvent.UserId}_{userEvent.Test}_{userEvent.GroupKey.OrDefault(userEvent.EventId)}";
    }
}
