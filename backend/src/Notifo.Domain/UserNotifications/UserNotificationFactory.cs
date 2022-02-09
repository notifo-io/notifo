﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.UserEvents;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.UserNotifications
{
    public class UserNotificationFactory : IUserNotificationFactory
    {
        private const string DefaultConfirmText = "Confirm";
        private readonly IUserNotificationUrl url;
        private readonly IClock clock;
        private readonly ILogStore logstore;

        public UserNotificationFactory(ILogStore logstore, IUserNotificationUrl url,
            IClock clock)
        {
            this.clock = clock;
            this.logstore = logstore;
            this.url = url;
        }

        public UserNotification? Create(App app, User user, UserEventMessage userEvent)
        {
            Guard.NotNull(user);
            Guard.NotNull(userEvent);

            if (userEvent.Formatting == null ||
                string.IsNullOrWhiteSpace(userEvent.AppId) ||
                string.IsNullOrWhiteSpace(userEvent.EventId) ||
                string.IsNullOrWhiteSpace(userEvent.Topic) ||
                string.IsNullOrWhiteSpace(userEvent.UserId))
            {
                return null;
            }

            var language = user.PreferredLanguage;

            if (!app.Languages.Contains(language))
            {
                logstore.LogAsync(app.Id, string.Format(CultureInfo.InvariantCulture, Texts.UserLanguage_NotValid, language, app.Language));

                language = app.Language;
            }

            var formatting = userEvent.Formatting.SelectText(language);

            if (!formatting.HasSubject())
            {
                return null;
            }

            var notification = SimpleMapper.Map(userEvent, new UserNotification
            {
                Id = Guid.NewGuid()
            });

            notification.UserLanguage = language;

            notification.Formatting = formatting;

            ConfigureTracking(notification, language, userEvent);
            ConfigureSettings(notification, user, userEvent);

            notification.Updated = clock.GetCurrentInstant();

            return notification;
        }

        private void ConfigureTracking(UserNotification notification, string language, UserEventMessage userEvent)
        {
            var confirmMode = userEvent.Formatting.ConfirmMode;

            if (confirmMode == ConfirmMode.Explicit)
            {
                notification.ConfirmUrl = url.TrackConfirmed(notification.Id, language);

                if (string.IsNullOrWhiteSpace(notification.Formatting.ConfirmText))
                {
                    notification.Formatting.ConfirmText = DefaultConfirmText;
                }
            }
            else
            {
                notification.Formatting.ConfirmText = null;
            }

            notification.TrackDeliveredUrl = url.TrackDelivered(notification.Id, language);
            notification.TrackSeenUrl = url.TrackSeen(notification.Id, language);
        }

        private static void ConfigureSettings(UserNotification notification, User user, UserEventMessage userEvent)
        {
            notification.Channels = new Dictionary<string, UserNotificationChannel>();

            OverrideBy(notification, user.Settings);
            OverrideBy(notification, userEvent.SubscriptionSettings);
            OverrideBy(notification, userEvent.EventSettings);
        }

        private static void OverrideBy(UserNotification notification, NotificationSettings? source)
        {
            if (source != null)
            {
                foreach (var (channelName, sourceSetting) in source)
                {
                    if (notification.Channels.TryGetValue(channelName, out var channel))
                    {
                        var setting = channel.Setting;

                        if (sourceSetting.Send != NotificationSend.Inherit && setting.Send != NotificationSend.NotAllowed)
                        {
                            setting.Send = sourceSetting.Send;
                        }

                        if (sourceSetting.DelayInSeconds.HasValue)
                        {
                            setting.DelayInSeconds = sourceSetting.DelayInSeconds;
                        }

                        if (sourceSetting.Properties?.Count > 0)
                        {
                            setting.Properties ??= new NotificationProperties();

                            foreach (var (key, value) in sourceSetting.Properties)
                            {
                                setting.Properties[key] = value;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(sourceSetting.Template))
                        {
                            setting.Template = sourceSetting.Template;
                        }
                    }
                    else
                    {
                        notification.Channels[channelName] = UserNotificationChannel.Create(sourceSetting);
                    }
                }
            }
        }
    }
}
