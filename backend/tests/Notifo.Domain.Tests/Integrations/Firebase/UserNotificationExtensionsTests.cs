// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Notifo.Domain.UserNotifications;
using Xunit;

namespace Notifo.Domain.Integrations.Firebase
{
    public class UserNotificationExtensionsTests
    {
        [Fact]
        public void Should_generate_firebase_message()
        {
            var id = Guid.NewGuid();
            var body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr";
            var confirmText = "Got It!";
            var confirmUrl = "https://confirm.notifo.com";
            var isConfirmed = true.ToString();
            var imageLarge = "https://via.placeholder.com/600";
            var imageSmall = "https://via.placeholder.com/100";
            var linkUrl = "https://app.notifo.io";
            var linkText = "Go to link";
            var silent = false.ToString();
            var subject = "subject1";
            var token = "token1";
            var trackingUrl = "https://track.notifo.com";
            var data = "data1";

            var notification = new UserNotification
            {
                Id = id,
                ConfirmUrl = confirmUrl,
                FirstConfirmed = new HandledInfo(),
                Formatting = new NotificationFormatting<string>
                {
                    Body = body,
                    ConfirmMode = ConfirmMode.Explicit,
                    ConfirmText = confirmText,
                    ImageLarge = imageLarge,
                    ImageSmall = imageSmall,
                    LinkText = linkText,
                    LinkUrl = linkUrl,
                    Subject = subject
                },
                Silent = false,
                TrackingUrl = trackingUrl,
                Data = data
            };

            var message = notification.ToFirebaseMessage(token, false);

            Assert.Equal(message.Token, token);
            Assert.Equal(message.Data[nameof(id)], id.ToString());
            Assert.Equal(message.Data[nameof(confirmText)], confirmText);
            Assert.Equal(message.Data[nameof(confirmUrl)], $"{confirmUrl}?channel=mobilepush&deviceIdentifier=token1");
            Assert.Equal(message.Data[nameof(isConfirmed)], isConfirmed);
            Assert.Equal(message.Data[nameof(imageSmall)], imageSmall);
            Assert.Equal(message.Data[nameof(imageLarge)], imageLarge);
            Assert.Equal(message.Data[nameof(linkText)], linkText);
            Assert.Equal(message.Data[nameof(linkUrl)], linkUrl);
            Assert.Equal(message.Data[nameof(silent)], silent);
            Assert.Equal(message.Data[nameof(trackingUrl)], $"{trackingUrl}?channel=mobilepush&deviceIdentifier=token1");
            Assert.Equal(message.Data[nameof(data)], data);

            Assert.Equal(message.Android.Data[nameof(subject)], subject);
            Assert.Equal(message.Android.Data[nameof(body)], body);

            Assert.Equal(message.Apns.Aps.Alert.Title, subject);
            Assert.Equal(message.Apns.Aps.Alert.Body, body);
            Assert.True(message.Apns.Aps.MutableContent);

            Assert.False(message.Apns.Aps.ContentAvailable);
        }

        [Fact]
        public void Should_not_include_empty_fields_in_firebase_message()
        {
            var id = Guid.NewGuid();
            var body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr";
            var confirmText = "Got It!";
            var confirmUrl = "https://confirm.notifo.com";
            var imageLarge = string.Empty;
            var imageSmall = (string?)null;
            var subject = "subject1";
            var token = "token1";
            var trackingUrl = (string?)null;

            var notification = new UserNotification
            {
                Id = id,
                ConfirmUrl = confirmUrl,
                Formatting = new NotificationFormatting<string>
                {
                    Body = body,
                    ConfirmMode = ConfirmMode.None,
                    ConfirmText = confirmText,
                    ImageLarge = imageLarge,
                    ImageSmall = imageSmall,
                    Subject = subject
                },
                TrackingUrl = trackingUrl
            };

            var message = notification.ToFirebaseMessage(token, false);

            Assert.Equal(message.Data[nameof(confirmText)], confirmText);
            Assert.Equal(message.Data[nameof(confirmUrl)], $"{confirmUrl}?channel=mobilepush&deviceIdentifier=token1");
            Assert.False(message.Data.ContainsKey(nameof(imageLarge)));
            Assert.False(message.Data.ContainsKey(nameof(imageSmall)));
            Assert.False(message.Data.ContainsKey(nameof(trackingUrl)));
            Assert.False(message.Apns.Aps.ContentAvailable);
        }

        [Fact]
        public void Should_create_silent_notification_when_flag_is_true()
        {
            var id = Guid.NewGuid();
            var token = "token1";

            var notification = new UserNotification
            {
                Id = id
            };

            var message = notification.ToFirebaseMessage(token, true);

            Assert.Equal(token, message.Token);
            Assert.Equal(message.Data[nameof(id)], id.ToString());
            Assert.Null(message.Notification);
        }

        [Fact]
        public void Should_include_time_to_live_property_if_set()
        {
            var token = "token1";
            var timeToLive = 1000;

            var notification = new UserNotification
            {
                Formatting = new NotificationFormatting<string>(),
                TimeToLiveInSeconds = timeToLive
            };

            var message = notification.ToFirebaseMessage(token, false);

            var unixTimeSeconds = DateTimeOffset.UtcNow.AddSeconds(timeToLive).ToUnixTimeSeconds();

            Assert.Equal(unixTimeSeconds, Convert.ToInt32(message.Apns.Headers["apns-expiration"]));
            Assert.Equal(timeToLive, message.Android.TimeToLive?.TotalSeconds);
        }

        [Fact]
        public void Should_not_include_time_to_live_property_if_not_set()
        {
            var token = "token1";

            var notification = new UserNotification
            {
                Formatting = new NotificationFormatting<string>()
            };

            var message = notification.ToFirebaseMessage(token, false);

            Assert.False(message.Apns.Headers.ContainsKey("apns-expiration"));
            Assert.Null(message.Android.TimeToLive);
        }

        [Fact]
        public void Should_set_time_to_live_to_zero_if_set_to_zero()
        {
            var token = "token1";
            var timeToLive = 0;

            var notification = new UserNotification
            {
                Formatting = new NotificationFormatting<string>(),
                TimeToLiveInSeconds = timeToLive
            };

            var message = notification.ToFirebaseMessage(token, false);

            Assert.Equal(timeToLive, Convert.ToInt32(message.Apns.Headers["apns-expiration"]));
            Assert.Equal(timeToLive, message.Android.TimeToLive?.TotalSeconds);
        }
    }
}
