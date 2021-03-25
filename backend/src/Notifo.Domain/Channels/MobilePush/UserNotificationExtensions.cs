// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FirebaseAdmin.Messaging;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.MobilePush
{
    public static class UserNotificationExtensions
    {
        public static Message ToFirebaseMessage(this UserNotification notification, string token)
        {
            var message = new Message
            {
                Token = token
            };

            var formatting = notification.Formatting;

            var commonData = new Dictionary<string, string>();
            commonData.AddIfPresent(nameof(notification.TrackingUrl), notification.TrackingUrl);
            commonData.AddIfPresent(nameof(notification.ConfirmUrl), notification.ConfirmUrl);
            commonData.AddIfPresent(nameof(formatting.ConfirmText), formatting.ConfirmText);
            commonData.AddIfPresent(nameof(formatting.ImageSmall), formatting.ImageSmall);
            commonData.AddIfPresent(nameof(formatting.ImageLarge), formatting.ImageLarge);

            message.Data = commonData;

            var androidData = new Dictionary<string, string>();
            androidData.AddIfPresent(nameof(formatting.Subject), formatting.Subject);
            androidData.AddIfPresent(nameof(formatting.Body), formatting.Body);

            message.Android = new AndroidConfig
            {
                Data = androidData
            };

            var apsAlert = new ApsAlert
            {
                Title = formatting.Subject
            };

            if (!string.IsNullOrWhiteSpace(formatting.Body))
            {
                apsAlert.Body = formatting.Body;
            }

            message.Apns = new ApnsConfig
            {
                Aps = new Aps
                {
                    Alert = apsAlert,
                    MutableContent = true
                }
            };

            return message;
        }

        private static void AddIfPresent(this IDictionary<string, string> dictionary, string propertyName, string? propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyValue))
            {
                return;
            }

            var key = $"{char.ToLowerInvariant(propertyName[0])}{propertyName[1..]}";
            dictionary[key] = propertyValue;
        }
    }
}
