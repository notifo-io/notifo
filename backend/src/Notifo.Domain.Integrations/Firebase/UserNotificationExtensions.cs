// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FirebaseAdmin.Messaging;
using Notifo.Domain.Channels;
using Notifo.Domain.UserNotifications;
using Squidex.Text;

namespace Notifo.Domain.Integrations.Firebase
{
    public static class UserNotificationExtensions
    {
        public static Message ToFirebaseMessage(this UserNotification notification, string token, bool wakeup)
        {
            var message = new Message
            {
                Token = token
            };

            if (wakeup)
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
                    .WithNonEmpty("isConfirmed", (notification.IsConfirmed != null).ToString())
                    .WithNonEmpty("imageLarge", formatting.ImageLarge)
                    .WithNonEmpty("imageSmall", formatting.ImageSmall)
                    .WithNonEmpty("linkText", formatting.LinkText)
                    .WithNonEmpty("linkUrl", formatting.LinkUrl)
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
                Headers = new Dictionary<string, string>
                {
                    ["apns-collapse-id"] = notification.Id.ToString()
                },
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
