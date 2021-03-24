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
            string token = "token1";
            string body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr";
            string subject = "subject1";
            string confirmText = "Got It!";
            string imageSmall = "https://via.placeholder.com/100";
            string imageLarge = "https://via.placeholder.com/600";
            string trackingUrl = "https://track.notifo.com";
            string confirmUrl = "https://confirm.notifo.com";

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

            message.Token.Should().BeEquivalentTo(token);
            message.Data[nameof(confirmText)].Should().BeEquivalentTo(confirmText);
            message.Data[nameof(imageSmall)].Should().BeEquivalentTo(imageSmall);
            message.Data[nameof(imageLarge)].Should().BeEquivalentTo(imageLarge);
            message.Data[nameof(trackingUrl)].Should().BeEquivalentTo(trackingUrl);
            message.Data[nameof(confirmUrl)].Should().BeEquivalentTo(confirmUrl);

            message.Android.Data[nameof(subject)].Should().BeEquivalentTo(subject);
            message.Android.Data[nameof(body)].Should().BeEquivalentTo(body);

            message.Apns.Aps.MutableContent.Should().BeTrue();
            message.Apns.Aps.Alert.Title.Should().BeEquivalentTo(subject);
            message.Apns.Aps.Alert.Body.Should().BeEquivalentTo(body);
        }

        [Fact]
        public void Should_not_include_empty_fields_in_firebase_message()
        {
            string token = "token1";
            string body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr";
            string subject = "subject1";
            string confirmText = "Got It!";
            string? imageSmall = null;
            string imageLarge = string.Empty;
            string? trackingUrl = null;
            string confirmUrl = "https://confirm.notifo.com";

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

            var data = (Dictionary<string, string>)message.Data;
            data[nameof(confirmText)].Should().BeEquivalentTo(confirmText);
            data[nameof(confirmUrl)].Should().BeEquivalentTo(confirmUrl);
            data.Should().NotContainKey(nameof(imageLarge));
            data.Should().NotContainKey(nameof(imageSmall));
            data.Should().NotContainKey(nameof(trackingUrl));
        }
    }
}
