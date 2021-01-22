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

        public async Task HandleAsync(List<UserEventMessage> jobs, bool isLastAttempt, CancellationToken ct)
        {
            foreach (var userEvent in jobs)
            {
                await DistributeScheduledAsync(userEvent, isLastAttempt);
            }
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

                var (notification, targets) = await CreateUserNotificationAsync(userEvent, user, app);

                try
                {
                    await userNotificationsStore.InsertAsync(notification);
                }
                catch (UniqueConstraintException)
                {
                    throw new DomainException(Texts.Notification_AlreadyProcessed);
                }

                foreach (var channel in targets)
                {
                    if (notification.Settings.TryGetValue(channel.Name, out var preference))
                    {
                        await channel.SendAsync(notification, preference, user, app, false);
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

        private async Task<(UserNotification, HashSet<ICommunicationChannel>)> CreateUserNotificationAsync(UserEventMessage userEvent, User user, App app)
        {
            var notification = userNotificationFactory.Create(app, user, userEvent);

            if (notification == null)
            {
                throw new DomainException(Texts.Notification_NoSubject);
            }

            var targets = new HashSet<ICommunicationChannel>();

            foreach (var channel in channels)
            {
                if (channel.IsSystem)
                {
                    var setting = new NotificationSetting
                    {
                        Send = NotificationSend.Send
                    };

                    if (channel.CanSend(notification, setting, user, app))
                    {
                        notification.Settings[channel.Name] = setting;

                        targets.Add(channel);
                    }
                }
                else
                {
                    if (notification.Settings.TryGetValue(channel.Name, out var preference) && preference.ShouldSend)
                    {
                        if (channel.CanSend(notification, preference, user, app))
                        {
                            notification.Sending[channel.Name] = new ChannelSendInfo
                            {
                                LastUpdate = clock.GetCurrentInstant()
                            };

                            targets.Add(channel);

                            await userNotificationsStore.CollectAsync(notification, channel.Name, ProcessStatus.Attempt);
                        }
                    }
                }
            }

            return (notification, targets);
        }

        public async Task<(UserNotification?, App?)> TrackConfirmedAsync(Guid id, string? sourceChannel = null)
        {
            var notification = await userNotificationsStore.TrackConfirmedAsync(id, sourceChannel);

            if (notification != null)
            {
                var app = await appStore.GetCachedAsync(notification.AppId, default);

                if (app == null)
                {
                    return (null, null);
                }

                if (notification.Settings.Values.Any(x => x.ShouldSend))
                {
                    var user = await userStore.GetCachedAsync(notification.AppId, notification.UserId);

                    if (user == null)
                    {
                        return (null, null);
                    }

                    foreach (var channel in channels)
                    {
                        if (notification.Settings.TryGetValue(channel.Name, out var preference))
                        {
                            await channel.SendAsync(notification, preference, user, app, true);
                        }
                    }
                }

                return (notification, app);
            }

            return (null, null);
        }

        public Task TrackSeenAsync(IEnumerable<Guid> ids, string? channel = null)
        {
            return userNotificationsStore.TrackSeenAsync(ids, channel);
        }

        private static string ScheduleKey(UserEventMessage userEvent)
        {
            return $"{userEvent.AppId}_{userEvent.UserId}_{userEvent.EventId}";
        }
    }
}
