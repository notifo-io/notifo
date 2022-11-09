// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using FirebaseAdmin.Messaging;
using Notifo.Domain.Channels;
using Notifo.Domain.UserNotifications;
using Squidex.Text;

namespace Notifo.Domain.Integrations.Firebase
{
    public static class UserNotificationExtensions
    {
        public static Message ToFirebaseMessage(this BaseUserNotification notification, string token, Guid configurationId, bool wakeup, bool isConfirmed)
        {
            var message = new Message
            {
                Token = token
            };

            if (wakeup)
            {
                message.Data =
                    new Dictionary<string, string>()
                        .WithNonEmpty("id", notification.Id.ToString());

                return message;
            }

            var formatting = notification.Formatting;

            message.Data =
                new Dictionary<string, string>()
                    .WithNonEmpty("id", notification.Id.ToString())
                    .WithNonEmpty("confirmText", formatting.ConfirmText)
                    .WithNonEmpty("confirmUrl", notification.ComputeConfirmUrl(Providers.MobilePush, configurationId))
                    .WithNonEmpty("isConfirmed", isConfirmed.ToString())
                    .WithNonEmpty("imageLarge", formatting.ImageLarge)
                    .WithNonEmpty("imageSmall", formatting.ImageSmall)
                    .WithNonEmpty("linkText", formatting.LinkText)
                    .WithNonEmpty("linkUrl", formatting.LinkUrl)
                    .WithNonEmpty("silent", notification.Silent.ToString())
                    .WithNonEmpty("trackDeliveredUrl", notification.ComputeTrackDeliveredUrl(Providers.MobilePush, configurationId))
                    .WithNonEmpty("trackSeenUrl", notification.ComputeTrackSeenUrl(Providers.MobilePush, configurationId))
                    .WithNonEmpty("trackingUrl", notification.ComputeTrackSeenUrl(Providers.MobilePush, configurationId))
                    .WithNonEmpty("data", notification.Data);

            var androidData =
                new Dictionary<string, string>()
                    .WithNonEmpty("subject", formatting.Subject)
                    .WithNonEmpty("body", formatting.Body);

            var androidConfig = new AndroidConfig
            {
                Data = androidData,
                Priority = Priority.High
            };

            var apsAlert = new ApsAlert
            {
                Title = formatting.Subject
            };

            if (!string.IsNullOrWhiteSpace(formatting.Body))
            {
                apsAlert.Body = formatting.Body;
            }

            var apnsHeaders = new Dictionary<string, string>
            {
                ["apns-collapse-id"] = notification.Id.ToString(),
                ["apns-push-type"] = "alert"
            };

            if (notification.TimeToLiveInSeconds is { } timeToLive)
            {
                androidConfig.TimeToLive = TimeSpan.FromSeconds(timeToLive);

                var unixTimeSeconds = DateTimeOffset.UtcNow.AddSeconds(timeToLive).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

                apnsHeaders["apns-expiration"] = timeToLive == 0 ? "0" : unixTimeSeconds;
            }

            var apnsConfig = new ApnsConfig
            {
                Headers = apnsHeaders,
                Aps = new Aps
                {
                    Alert = apsAlert,
                    MutableContent = true
                }
            };

            message.Android = androidConfig;
            message.Apns = apnsConfig;

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
