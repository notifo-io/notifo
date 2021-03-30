// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FirebaseAdmin.Messaging;
using Notifo.Domain.UserNotifications;
using Squidex.Text;

namespace Notifo.Domain.Channels.MobilePush
{
    public static class UserNotificationExtensions
    {
        public static Message ToFirebaseMessage(this UserNotification notification, string token, bool sendWakeup)
        {
            var message = new Message
            {
                Token = token
            };

            if (sendWakeup)
            {
                message.Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        ContentAvailable = true
                    },
                    Headers = new Dictionary<string, string>
                    {
                        ["apns-priority"] = "5"
                    }
                };

                return message;
            }

            var formatting = notification.Formatting;

            message.Data =
                new Dictionary<string, string>()
                    .WithNonEmpty("id", notification.Id.ToString())
                    .WithNonEmpty("confirmText", formatting.ConfirmText)
                    .WithNonEmpty("confirmUrl", notification.ComputeConfirmUrl(Providers.MobilePush, token))
                    .WithNonEmpty("imageLarge", formatting.ImageLarge)
                    .WithNonEmpty("imageSmall", formatting.ImageSmall)
                    .WithNonEmpty("trackingUrl", notification.ComputeTrackingUrl(Providers.MobilePush, token));

            var androidData =
                new Dictionary<string, string>()
                    .WithNonEmpty("subject", formatting.Subject)
                    .WithNonEmpty("body", formatting.Body);

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

        private static Dictionary<string, string> WithNonEmpty(this Dictionary<string, string> dictionary, string propertyName, string? propertyValue)
        {
            if (!string.IsNullOrWhiteSpace(propertyValue))
            {
                dictionary[propertyName.ToCamelCase()] = propertyValue;
            }

            return dictionary;
        }
    }
}
