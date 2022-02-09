﻿// ==========================================================================
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
using Notifo.Infrastructure.Messaging;
using Notifo.Infrastructure.Scheduling;
using IUserEventQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.UserEvents.UserEventMessage>;

namespace Notifo.Domain.UserNotifications
{
    public sealed class UserNotificationService : IUserNotificationService, IScheduleHandler<UserEventMessage>, IMessageHandler<ConfirmMessage>
    {
        private readonly IAppStore appStore;
        private readonly IClock clock;
        private readonly IEnumerable<ICommunicationChannel> channels;
        private readonly ILogger<UserNotificationService> log;
        private readonly ILogStore logStore;
        private readonly IMessageProducer<ConfirmMessage> confirmProducer;
        private readonly IUserEventQueue userEventQueue;
        private readonly IUserNotificationFactory userNotificationFactory;
        private readonly IUserNotificationStore userNotificationsStore;
        private readonly IUserStore userStore;

        public UserNotificationService(IEnumerable<ICommunicationChannel> channels,
            IAppStore appStore,
            ILogStore logStore,
            IMessageProducer<ConfirmMessage> confirmProducer,
            IUserEventQueue userEventQueue,
            IUserNotificationFactory userNotificationFactory,
            IUserNotificationStore userNotificationsStore,
            IUserStore userStore,
            ILogger<UserNotificationService> log,
            IClock clock)
        {
            this.appStore = appStore;
            this.channels = channels;
            this.log = log;
            this.logStore = logStore;
            this.confirmProducer = confirmProducer;
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
                    await logStore.LogAsync(userEvent.AppId, ex.Message);

                    await userNotificationsStore.TrackFailedAsync(userEvent);
                }
            }
        }

        public async Task DistributeScheduledAsync(UserEventMessage userEvent, bool isLastAttempt)
        {
            var links = userEvent.Links();

            var parentContext = Activity.Current?.Context ?? default;

            using (var activity = Telemetry.Activities.StartActivity("DistributeUserEventScheduled", ActivityKind.Internal, parentContext, links: links))
            {
                await userNotificationsStore.TrackAttemptAsync(userEvent);

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

                    var options = new SendOptions { App = app, User = user };

                    var notification = await CreateUserNotificationAsync(userEvent, options);

                    notification.NotificationActivity = activity?.Context ?? default;

                    try
                    {
                        await userNotificationsStore.InsertAsync(notification);
                    }
                    catch (UniqueConstraintException)
                    {
                        throw new DomainException(Texts.Notification_AlreadyProcessed);
                    }

                    foreach (var channel in channels)
                    {
                        if (notification.Channels.TryGetValue(channel.Name, out var notificationChannel))
                        {
                            foreach (var configuration in notificationChannel.Status.Keys)
                            {
                                await channel.SendAsync(notification, notificationChannel.Setting, configuration, options, default);
                            }
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
                        await userNotificationsStore.TrackFailedAsync(userEvent);
                    }

                    if (ex is DomainException domainException)
                    {
                        await logStore.LogAsync(userEvent.AppId, domainException.Message);
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

        private async Task<UserNotification> CreateUserNotificationAsync(UserEventMessage userEvent, SendOptions options)
        {
            using (Telemetry.Activities.StartActivity("CreateUserNotification"))
            {
                var notification = userNotificationFactory.Create(options.App, options.User, userEvent);

                if (notification == null)
                {
                    throw new DomainException(Texts.Notification_NoSubject);
                }

                foreach (var channel in channels)
                {
                    if (channel.IsSystem && !string.IsNullOrWhiteSpace(channel.Name))
                    {
                        if (!notification.Channels.TryGetValue(channel.Name, out var systemConfig))
                        {
                            systemConfig = UserNotificationChannel.Create();

                            notification.Channels[channel.Name] = systemConfig;
                        }

                        systemConfig.Setting.Send = NotificationSend.Send;
                    }

                    if (notification.Channels.TryGetValue(channel.Name, out var channelConfig) && channelConfig.Setting.ShouldSend)
                    {
                        var configurations = channel.GetConfigurations(notification, channelConfig.Setting, options);

                        foreach (var configuration in configurations)
                        {
                            if (!string.IsNullOrWhiteSpace(configuration))
                            {
                                channelConfig.Status[configuration] = new ChannelSendInfo();

                                await userNotificationsStore.CollectAsync(notification, channel.Name, ProcessStatus.Attempt);
                            }
                        }
                    }
                }

                return notification;
            }
        }

        public async Task HandleAsync(ConfirmMessage message,
            CancellationToken ct = default)
        {
            var notification = await userNotificationsStore.TrackConfirmedAsync(message.Id, message.Details, ct);

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

                var options = new SendOptions { App = app, User = user, IsUpdate = true };

                foreach (var channel in channels)
                {
                    if (notification.Channels.TryGetValue(channel.Name, out var notificationChannel))
                    {
                        foreach (var configuration in notificationChannel.Status.Keys)
                        {
                            await channel.SendAsync(notification, notificationChannel.Setting, configuration, options, ct);
                        }
                    }
                }
            }
            catch (DomainException domainException)
            {
                await logStore.LogAsync(notification.AppId, domainException.Message);
                throw;
            }
        }

        public async Task TrackConfirmedAsync(Guid id, TrackingDetails details)
        {
            await confirmProducer.ProduceAsync(id.ToString(), new ConfirmMessage { Id = id, Details = details });
        }

        public async Task TrackDeliveredAsync(IEnumerable<Guid> ids, TrackingDetails details)
        {
            await userNotificationsStore.TrackDeliveredAsync(ids, details);

            if (!string.IsNullOrWhiteSpace(details.Channel))
            {
                var channel = channels.FirstOrDefault(x => x.Name == details.Channel);

                if (channel != null)
                {
                    foreach (var id in ids)
                    {
                        await channel.HandleDeliveredAsync(id, details);
                    }
                }
            }
        }

        public async Task TrackSeenAsync(IEnumerable<Guid> ids, TrackingDetails details)
        {
            await userNotificationsStore.TrackSeenAsync(ids, details);

            if (!string.IsNullOrWhiteSpace(details.Channel))
            {
                var channel = channels.FirstOrDefault(x => x.Name == details.Channel);

                if (channel != null)
                {
                    foreach (var id in ids)
                    {
                        await channel.HandleSeenAsync(id, details);
                    }
                }
            }
        }

        private static string ScheduleKey(UserEventMessage userEvent)
        {
            return $"{userEvent.AppId}_{userEvent.UserId}_{userEvent.EventId}";
        }
    }
}
