// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using FirebaseAdmin.Messaging;
using Jint;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Integrations.Firebase;

[UsesVerify]
public class UserNotificationExtensionsTests
{
    private static readonly Guid Id = Guid.Parse("d9e3afe8-06a2-493e-b7a7-03a4aba61544");
    private readonly string token = "token1";

    [Fact]
    public Task Should_generate_firebase_message()
    {
        var notification = new UserNotification
        {
            Id = Id,
            ConfirmUrl = "https://confirm.notifo.com",
            FirstConfirmed = new HandledInfo(default, null),
            FirstDelivered = new HandledInfo(default, null),
            Formatting = new NotificationFormatting<string>
            {
                Body = "My Body",
                ConfirmMode = ConfirmMode.Explicit,
                ConfirmText = "My Confirm",
                ImageLarge = "https://via.placeholder.com/600",
                ImageSmall = "https://via.placeholder.com/100",
                LinkText = "My Link",
                LinkUrl = "https://app.notifo.io",
                Subject = "My Subject"
            },
            Silent = false,
            TrackDeliveredUrl = "https://track-delivered.notifo.com",
            TrackSeenUrl = "https://track-seen.notifo.com",
            Data = "data1"
        };

        var message = notification.ToFirebaseMessage(token, Id, false, true);

        return Verify(message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_not_include_empty_fields_in_firebase_message(string? emptyText)
    {
        var notification = new UserNotification
        {
            Id = Id,
            ConfirmUrl = "https://confirm.notifo.com",
            Formatting = new NotificationFormatting<string>
            {
                Body = emptyText,
                ConfirmMode = ConfirmMode.None,
                ConfirmText = emptyText,
                ImageLarge = emptyText,
                ImageSmall = emptyText,
                Subject = "My Subject",
            },
            TrackDeliveredUrl = emptyText,
            TrackSeenUrl = emptyText
        };

        var message = notification.ToFirebaseMessage(token, Id, false, false);

        Assert.False(message.Apns.Aps.ContentAvailable);
        Assert.DoesNotContain("body", message.Data);
        Assert.DoesNotContain("imageLarge", message.Data);
        Assert.DoesNotContain("imageSmall", message.Data);
        Assert.DoesNotContain("trackDeliveredUrl", message.Data);
        Assert.DoesNotContain("trackingUrl", message.Data);
    }

    [Fact]
    public Task Should_create_silent_notification_when_flag_is_true()
    {
        var notification = new UserNotification
        {
            Id = Id
        };

        var message = notification.ToFirebaseMessage(token, Id, true, false);

        return Verify(message);
    }

    [Fact]
    public Task Should_include_time_to_live_property_if_set()
    {
        var timeToLive = 1000;

        var notification = new UserNotification
        {
            Formatting = new NotificationFormatting<string>(),
            TrackDeliveredUrl = null,
            TrackSeenUrl = null,
            TimeToLiveInSeconds = timeToLive
        };

        var message = notification.ToFirebaseMessage(token, Id, false, false);

        var timeToLiveSec = DateTimeOffset.UtcNow.AddSeconds(timeToLive).ToUnixTimeSeconds();

        Assert.Equal(timeToLiveSec, int.Parse(message.Apns.Headers["apns-expiration"], NumberStyles.Integer, CultureInfo.InvariantCulture));
        Assert.Equal(timeToLive, message.Android.TimeToLive?.TotalSeconds);

        return Verify(message).IgnoreMember("apns-expiration");
    }

    [Fact]
    public Task Should_set_time_to_live_to_zero_if_set_to_zero()
    {
        var timeToLive = 0;

        var notification = new UserNotification
        {
            Formatting = new NotificationFormatting<string>(),
            TrackDeliveredUrl = null,
            TrackSeenUrl = null,
            TimeToLiveInSeconds = timeToLive
        };

        var message = notification.ToFirebaseMessage(token, Id, false, false);

        Assert.Equal(0, int.Parse(message.Apns.Headers["apns-expiration"], NumberStyles.Integer, CultureInfo.InvariantCulture));
        Assert.Equal(0, message.Android.TimeToLive?.TotalSeconds);

        return Verify(message).IgnoreMember("apns-expiration");
    }

    [Fact]
    public Task Should_not_include_time_to_live_property_if_not_set()
    {
        var notification = new UserNotification
        {
            Formatting = new NotificationFormatting<string>()
        };

        var message = notification.ToFirebaseMessage(token, Id, false, false);

        AssertTTL(message, -1);

        return Verify(message).IgnoreMember("apns-expiration");
    }

    private static void AssertTTL(Message message, int timeToLive)
    {
        if (timeToLive >= 0)
        {
            var timeToLiveSec = DateTimeOffset.UtcNow.AddSeconds(timeToLive).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

            Assert.Equal(timeToLiveSec, message.Apns.Headers["apns-expiration"]);
            Assert.Equal(timeToLive, message.Android.TimeToLive?.TotalSeconds);
        }
        else
        {
            Assert.DoesNotContain("apns-expiration", message.Apns.Headers);
            Assert.Null(message.Android.TimeToLive);
        }
    }
}
