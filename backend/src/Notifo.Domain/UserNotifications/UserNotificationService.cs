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
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.UserEvents;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using Squidex.Messaging;
using IUserEventQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.UserEvents.UserEventMessage>;

namespace Notifo.Domain.UserNotifications;

public sealed class UserNotificationService : IUserNotificationService, IScheduleHandler<UserEventMessage>, IMessageHandler<ConfirmMessage>
{
    private readonly IAppStore appStore;
    private readonly IClock clock;
    private readonly Dictionary<string, ICommunicationChannel> channels;
    private readonly ILogger<UserNotificationService> log;
    private readonly ILogStore logStore;
    private readonly IMessageBus messageBus;
    private readonly IUserEventQueue userEventQueue;
    private readonly IUserNotificationFactory userNotificationFactory;
    private readonly IUserNotificationStore userNotificationsStore;
    private readonly IUserStore userStore;

    public UserNotificationService(IEnumerable<ICommunicationChannel> channels,
        IAppStore appStore,
        IMessageBus messageBus,
        IUserEventQueue userEventQueue,
        IUserNotificationFactory userNotificationFactory,
        IUserNotificationStore userNotificationsStore,
        IUserStore userStore,
        ILogStore logStore,
        ILogger<UserNotificationService> log,
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

    public async Task<bool> HandleAsync(UserEventMessage job, bool isLastAttempt,
        CancellationToken ct)
    {
        await DistributeScheduledAsync(job, isLastAttempt);

        return true;
    }

    public async Task DistributeAsync(UserEventMessage userEvent)
    {
        using (Telemetry.Activities.StartActivity("HandleUserEvent"))
        {
            try
            {
                var user = await userStore.GetCachedAsync(userEvent.AppId, userEvent.UserId);

                if (user == null)
                {
                    throw new DomainException(Texts.Notification_NoUser);
                }

                var dueTime = Scheduling.CalculateScheduleTime(userEvent.Scheduling, clock, user.PreferredTimezone);

                await userEventQueue.ScheduleAsync(ScheduleKey(userEvent), userEvent, dueTime, true);
            }
            catch (DomainException ex)
            {
                await logStore.LogAsync(userEvent.AppId, LogMessage.General_Exception("System", ex));

                await userNotificationsStore.TrackAsync(userEvent, ProcessStatus.Failed);
            }
        }
    }

    public async Task DistributeScheduledAsync(UserEventMessage userEvent, bool isLastAttempt)
    {
        var links = userEvent.Links();

        var parentContext = Activity.Current?.Context ?? default;

        using (var activity = Telemetry.Activities.StartActivity("DistributeUserEventScheduled", ActivityKind.Internal, parentContext, links: links))
        {
            await userNotificationsStore.TrackAsync(userEvent, ProcessStatus.Attempt);

            try
            {
                var user = await userStore.GetCachedAsync(userEvent.AppId, userEvent.UserId);

                if (user == null)
                {
                    throw new DomainException(Texts.Notification_NoApp);
                }

                var app = await appStore.GetCachedAsync(userEvent.AppId);

                if (app == null)
                {
                    throw new DomainException(Texts.Notification_NoUser);
                }

                var context = new SendContext
                {
                    App = app,
                    AppId = app.Id,
                    User = user,
                    UserId = user.Id,
                    IsUpdate = false
                };

                var notification = await CreateUserNotificationAsync(userEvent, context);

                // Assign the notification activity, so that we can continue with that when the handle the event.
                notification.UserNotificationActivity = activity?.Context ?? default;

                try
                {
                    await userNotificationsStore.InsertAsync(notification);
                }
                catch (UniqueConstraintException)
                {
                    throw new DomainException(Texts.Notification_AlreadyProcessed);
                }

                foreach (var channel in channels.Values)
                {
                    if (!notification.Channels.TryGetValue(channel.Name, out var notificationChannel))
                    {
                        continue;
                    }

                    foreach (var (id, status) in notificationChannel.Status)
                    {
                        await channel.SendAsync(notification, notificationChannel.Setting, id, status.Configuration, context, default);
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
                    await userNotificationsStore.TrackAsync(userEvent, ProcessStatus.Failed);
                }

                if (ex is DomainException domainException)
                {
                    await logStore.LogAsync(userEvent.AppId, LogMessage.General_Exception("System", domainException));
                }
                else
                {
                    log.LogError(ex, "Failed to process user event for app {appId} with ID {id} to topic {topic}.",
                        userEvent.AppId,
                        userEvent.EventId,
                        userEvent.Topic);
                    throw;
                }
            }
        }
    }

    private async Task<UserNotification> CreateUserNotificationAsync(UserEventMessage userEvent, SendContext context)
    {
        using (Telemetry.Activities.StartActivity("CreateUserNotification"))
        {
            var notification = userNotificationFactory.Create(context.App, context.User, userEvent);

            if (notification == null)
            {
                throw new DomainException(Texts.Notification_NoSubject);
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
                    var configurations = channel.GetConfigurations(notification, channelConfig.Setting, context);
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
            var app = await appStore.GetCachedAsync(notification.AppId, ct);

            if (app == null)
            {
                return;
            }

            var user = await userStore.GetCachedAsync(notification.AppId, notification.UserId, ct);

            if (user == null)
            {
                throw new DomainException(Texts.Notification_NoUser);
            }

            var context = new SendContext
            {
                App = app,
                AppId = app.Id,
                User = user,
                UserId = user.Id,
                IsUpdate = true
            };

            foreach (var channel in channels.Values)
            {
                if (!notification.Channels.TryGetValue(channel.Name, out var channelInfo))
                {
                    continue;
                }

                foreach (var (id, status) in channelInfo.Status)
                {
                    await channel.SendAsync(notification, channelInfo.Setting, id, status.Configuration, context, ct);
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
            // If the notification has not been updated this tracking has happened before.
            if (!updated)
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

                var numConfiguration = notification.Channels.GetOrAddDefault(token.Channel)?.Status.Count ?? 0;

                // There is no configuration for this channel, so the notification has never been sent over.
                if (numConfiguration == 0)
                {
                    continue;
                }

                if (!channels.TryGetValue(token.Channel, out var channel))
                {
                    continue;
                }

                await channel.HandleSeenAsync(notification, token.ConfigurationId);
            }
        }
    }

    private static string ScheduleKey(UserEventMessage userEvent)
    {
        return $"{userEvent.AppId}_{userEvent.UserId}_{userEvent.EventId}";
    }
}
