// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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

            var commonData = new Dictionary<string, string>();
            commonData.AddIfPresent(() => notification.TrackingUrl);
            commonData.AddIfPresent(() => notification.ConfirmUrl);
            commonData.AddIfPresent(() => notification.Formatting.ConfirmText);
            commonData.AddIfPresent(() => notification.Formatting.ImageSmall);
            commonData.AddIfPresent(() => notification.Formatting.ImageLarge);

            message.Data = commonData;

            var androidData = new Dictionary<string, string>();
            androidData.AddIfPresent(() => notification.Formatting.Subject);
            androidData.AddIfPresent(() => notification.Formatting.Body);

            message.Android = new AndroidConfig
            {
                Data = androidData
            };

            var apsAlert = new ApsAlert
            {
                Title = notification.Formatting.Subject,
                Body = notification.Formatting.Body ?? string.Empty
            };

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

        private static void AddIfPresent(this IDictionary<string, string> dictionary, Expression<Func<string?>> expression)
        {
            var value = expression.Compile().Invoke();
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var propertyInfo = ((MemberExpression)expression.Body).Member as PropertyInfo;
            if (propertyInfo == null)
            {
                return;
            }

            string name = propertyInfo.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            string key = $"{char.ToLowerInvariant(name[0])}{name[1..]}";
            dictionary[key] = value;
        }
    }
}
