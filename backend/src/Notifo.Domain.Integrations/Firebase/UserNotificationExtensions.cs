// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using FirebaseAdmin.Messaging;
using Squidex.Text;

namespace Notifo.Domain.Integrations.Firebase;

public static class UserNotificationExtensions
{
    public static Message ToFirebaseMessage(this MobilePushMessage source, DateTimeOffset now)
    {
        var message = new Message
        {
            Token = source.DeviceToken
        };

        if (source.Wakeup)
        {
            message.Data =
                new Dictionary<string, string>()
                    .WithNonEmpty("id", source.NotificationId.ToString());

            message.Apns = new ApnsConfig
            {
                Headers =  new Dictionary<string, string>
                {
                    ["apns-push-type"] = "background",
                    ["apns-priority"] = "5"
                },
                Aps = new Aps
                {
                    ContentAvailable = true
                }
            };

            return message;
        }

        message.Data =
            new Dictionary<string, string>()
                .WithNonEmpty("id", source.NotificationId.ToString())
                .WithNonEmpty("confirmText", source.ConfirmText)
                .WithNonEmpty("confirmUrl", source.ConfirmUrl)
                .WithNonEmpty("isConfirmed", source.IsConfirmed.ToString())
                .WithNonEmpty("imageLarge", source.ImageLarge)
                .WithNonEmpty("imageSmall", source.ImageSmall)
                .WithNonEmpty("linkText", source.LinkText)
                .WithNonEmpty("linkUrl", source.LinkUrl)
                .WithNonEmpty("silent", source.Silent.ToString())
                .WithNonEmpty("trackingToken", source.TrackingToken)
                .WithNonEmpty("trackDeliveredUrl", source.TrackDeliveredUrl)
                .WithNonEmpty("trackSeenUrl", source.TrackSeenUrl)
                .WithNonEmpty("trackingUrl", source.TrackSeenUrl)
                .WithNonEmpty("data", source.Data);

        var androidConfig = new AndroidConfig
        {
            Data =
                new Dictionary<string, string>()
                    .WithNonEmpty("subject", source.Subject)
                    .WithNonEmpty("body", source.Body),
            Priority = Priority.High
        };

        var apsAlert = new ApsAlert
        {
            Title = source.Subject
        };

        if (!string.IsNullOrWhiteSpace(source.Body))
        {
            apsAlert.Body = source.Body;
        }

        var apnsHeaders = new Dictionary<string, string>
        {
            ["apns-collapse-id"] = source.NotificationId.ToString()!,
            ["apns-push-type"] = "alert"
        };

        if (source.TimeToLiveInSeconds is { } timeToLive)
        {
            androidConfig.TimeToLive = TimeSpan.FromSeconds(timeToLive);

            var unixTimeSeconds = now.AddSeconds(timeToLive).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

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
