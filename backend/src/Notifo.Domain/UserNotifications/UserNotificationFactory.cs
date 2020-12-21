// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
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

        public UserNotificationFactory(IClock clock, ILogStore logstore, IUserNotificationUrl url)
        {
            this.clock = clock;
            this.logstore = logstore;
            this.url = url;
        }

        public UserNotification? Create(App app, User user, UserEventMessage userEvent)
        {
            Guard.NotNull(user, nameof(user));
            Guard.NotNull(userEvent, nameof(userEvent));

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
                logstore.LogAsync(app.Id, string.Format(Texts.UserLanguage_NotValid, language, app.Language));

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

            notification.Formatting = formatting;
            notification.UserLanguage = language;

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

            notification.TrackingUrl = url.TrackSeen(notification.Id, language);
        }

        private static void ConfigureSettings(UserNotification notification, User user, UserEventMessage userEvent)
        {
            notification.Settings = new NotificationSettings();
            notification.Settings.OverrideBy(user.Settings);
            notification.Settings.OverrideBy(userEvent.SubscriptionSettings);
            notification.Settings.OverrideBy(userEvent.EventSettings);

            notification.Sending = new Dictionary<string, ChannelSendInfo>();
        }
    }
}
