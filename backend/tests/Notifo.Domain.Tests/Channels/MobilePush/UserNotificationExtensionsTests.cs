// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Notifo.Domain.UserNotifications;
using Xunit;

namespace Notifo.Domain.Channels.MobilePush
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
            var imageLarge = "https://via.placeholder.com/600";
            var imageSmall = "https://via.placeholder.com/100";
            var subject = "subject1";
            var token = "token1";
            var trackingUrl = "https://track.notifo.com";

            var notification = new UserNotification
            {
                Id = id,
                ConfirmUrl = confirmUrl,
                Formatting = new NotificationFormatting<string>
                {
                    Body = body,
                    ConfirmMode = ConfirmMode.Explicit,
                    ConfirmText = confirmText,
                    ImageLarge = imageLarge,
                    ImageSmall = imageSmall,
                    Subject = subject
                },
                TrackingUrl = trackingUrl
            };

            var message = notification.ToFirebaseMessage(token, false);

            Assert.Equal(message.Token, token);
            Assert.Equal(message.Data[nameof(id)], id.ToString());
            Assert.Equal(message.Data[nameof(confirmText)], confirmText);
            Assert.Equal(message.Data[nameof(confirmUrl)], $"{confirmUrl}?channel=mobilepush&deviceIdentifier=token1");
            Assert.Equal(message.Data[nameof(imageSmall)], imageSmall);
            Assert.Equal(message.Data[nameof(imageLarge)], imageLarge);
            Assert.Equal(message.Data[nameof(trackingUrl)], $"{$"{trackingUrl}?channel=mobilepush&deviceIdentifier=token1"}");

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
            Assert.Equal(message.Data[nameof(confirmUrl)], $"{$"{confirmUrl}?channel=mobilepush&deviceIdentifier=token1"}");
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
            Assert.True(message.Apns.Aps.ContentAvailable);
        }
    }
}
