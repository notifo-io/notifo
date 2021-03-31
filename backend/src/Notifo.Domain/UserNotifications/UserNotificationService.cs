// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.UserEvents;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;
using Squidex.Hosting;
using IUserEventQueue = Notifo.Infrastructure.Scheduling.IScheduler<Notifo.Domain.UserEvents.UserEventMessage>;

namespace Notifo.Domain.UserNotifications
{
    public sealed class UserNotificationService : IInitializable, IUserNotificationService, IScheduleHandler<UserEventMessage>
    {
        private readonly IEnumerable<ICommunicationChannel> channels;
        private readonly IAppStore appStore;
        private readonly IUserStore userStore;
        private readonly IUserNotificationStore userNotificationsStore;
        private readonly IUserNotificationFactory userNotificationFactory;
        private readonly IUserEventQueue userEventQueue;
        private readonly ILogStore logStore;
        private readonly IClock clock;

        public int Order => 1000;

        public UserNotificationService(IEnumerable<ICommunicationChannel> channels,
            IAppStore appStore,
            ILogStore logStore,
            IUserEventQueue userEventQueue,
            IUserNotificationFactory userNotificationFactory,
            IUserNotificationStore userNotificationsStore,
            IUserStore userStore,
            IClock clock)
        {
            this.appStore = appStore;
            this.channels = channels;
            this.logStore = logStore;
            this.userEventQueue = userEventQueue;
            this.userNotificationFactory = userNotificationFactory;
            this.userNotificationsStore = userNotificationsStore;
            this.userStore = userStore;
            this.clock = clock;
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            userEventQueue.Subscribe(this);

            return Task.CompletedTask;
        }

        public async Task<bool> HandleAsync(UserEventMessage job, bool isLastAttempt, CancellationToken ct)
        {
            await DistributeScheduledAsync(job, isLastAttempt);

            return true;
        }

        public async Task DistributeAsync(UserEventMessage userEvent)
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

        public async Task DistributeScheduledAsync(UserEventMessage userEvent, bool isLastAttempt)
        {
            await userNotificationsStore.TrackAttemptAsync(userEvent);

            try
            {
                var user = await userStore.GetCachedAsync(userEvent.AppId, userEvent.UserId);

                if (user == null)
                {
                    throw new DomainException(Texts.Notification_NoApp);
                }

                var app = await appStore.GetCachedAsync(userEvent.AppId, default);

                if (app == null)
                {
                    throw new DomainException(Texts.Notification_NoUser);
                }

                var options = new SendOptions { App = app, User = user };

                var notification = await CreateUserNotificationAsync(userEvent, options);

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
                    throw;
                }
            }
        }

        private async Task<UserNotification> CreateUserNotificationAsync(UserEventMessage userEvent, SendOptions options)
        {
            var notification = userNotificationFactory.Create(options.App, options.User, userEvent);

            if (notification == null)
            {
                throw new DomainException(Texts.Notification_NoSubject);
            }

            foreach (var channel in channels)
            {
                if (channel.IsSystem)
                {
                    var setting = new NotificationSetting
                    {
                        Send = NotificationSend.Send
                    };

                    notification.Channels[channel.Name] = UserNotificationChannel.Create(new NotificationSetting
                    {
                        Send = NotificationSend.Send
                    });
                }

                if (notification.Channels.TryGetValue(channel.Name, out var notificationChannel) && notificationChannel.Setting.ShouldSend)
                {
                    var configurations = channel.GetConfigurations(notification, notificationChannel.Setting, options);

                    foreach (var configuration in configurations)
                    {
                        notificationChannel.Status[configuration] = new ChannelSendInfo();

                        await userNotificationsStore.CollectAsync(notification, channel.Name, ProcessStatus.Attempt);
                    }
                }
            }

            return notification;
        }

        public async Task<(UserNotification?, App?)> TrackConfirmedAsync(Guid id, TrackingDetails details)
        {
            var notification = await userNotificationsStore.TrackConfirmedAsync(id, details);

            if (notification != null)
            {
                var app = await appStore.GetCachedAsync(notification.AppId, default);

                if (app == null)
                {
                    return (null, null);
                }

                if (notification.Channels.Any())
                {
                    var user = await userStore.GetCachedAsync(notification.AppId, notification.UserId);

                    if (user == null)
                    {
                        return (null, null);
                    }

                    var options = new SendOptions { App = app, User = user, IsUpdate = true };

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
                }

                return (notification, app);
            }

            return (null, null);
        }

        public async Task TrackSeenAsync(IEnumerable<Guid> ids, TrackingDetails details)
        {
            await userNotificationsStore.TrackSeenAsync(ids, details);

            foreach (var channel in channels)
            {
                if (channel.Name == details.Channel)
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
