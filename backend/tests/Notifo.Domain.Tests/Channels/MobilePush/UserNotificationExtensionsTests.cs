// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Notifo.Domain.UserNotifications;
using Xunit;

namespace Notifo.Domain.Channels.MobilePush
{
    public class UserNotificationExtensionsTests
    {
        [Fact]
        public void Should_generate_firebase_message()
        {
            var token = "token1";
            var body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr";
            var subject = "subject1";
            var confirmText = "Got It!";
            var imageSmall = "https://via.placeholder.com/100";
            var imageLarge = "https://via.placeholder.com/600";
            var trackingUrl = "https://track.notifo.com";
            var confirmUrl = "https://confirm.notifo.com";

            var notification = new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Body = body,
                    Subject = subject,
                    ImageSmall = imageSmall,
                    ImageLarge = imageLarge,
                    ConfirmText = confirmText,
                    ConfirmMode = ConfirmMode.Explicit
                },
                TrackingUrl = trackingUrl,
                ConfirmUrl = confirmUrl
            };

            var message = notification.ToFirebaseMessage(token);

            Assert.Equal(message.Token, token);
            Assert.Equal(message.Data[nameof(confirmText)], confirmText);
            Assert.Equal(message.Data[nameof(imageSmall)], imageSmall);
            Assert.Equal(message.Data[nameof(imageLarge)], imageLarge);
            Assert.Equal(message.Data[nameof(trackingUrl)], trackingUrl);
            Assert.Equal(message.Data[nameof(confirmUrl)], confirmUrl);

            Assert.Equal(message.Android.Data[nameof(subject)], subject);
            Assert.Equal(message.Android.Data[nameof(body)], body);

            Assert.Equal(message.Apns.Aps.Alert.Title, subject);
            Assert.Equal(message.Apns.Aps.Alert.Body, body);
            Assert.True(message.Apns.Aps.MutableContent);
        }

        [Fact]
        public void Should_not_include_empty_fields_in_firebase_message()
        {
            var token = "token1";
            var body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr";
            var subject = "subject1";
            var confirmText = "Got It!";
            string? imageSmall = null;
            var imageLarge = string.Empty;
            string? trackingUrl = null;
            var confirmUrl = "https://confirm.notifo.com";

            var notification = new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Body = body,
                    Subject = subject,
                    ImageSmall = imageSmall,
                    ImageLarge = imageLarge,
                    ConfirmText = confirmText,
                },
                TrackingUrl = trackingUrl,
                ConfirmUrl = confirmUrl
            };

            var message = notification.ToFirebaseMessage(token);

            Assert.Equal(message.Data[nameof(confirmText)], confirmText);
            Assert.Equal(message.Data[nameof(confirmUrl)], confirmUrl);
            Assert.False(message.Data.ContainsKey(nameof(imageLarge)));
            Assert.False(message.Data.ContainsKey(nameof(imageSmall)));
            Assert.False(message.Data.ContainsKey(nameof(trackingUrl)));
        }
    }
}
