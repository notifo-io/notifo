// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using FirebaseAdmin.Messaging;

namespace Notifo.Domain.Integrations.Firebase;

[UsesVerify]
public class UserNotificationExtensionsTests
{
    private readonly Guid id = Guid.Parse("d9e3afe8-06a2-493e-b7a7-03a4aba61544");
    private readonly DateTimeOffset now = new DateTimeOffset(2022, 12, 11, 10, 9, 8, default);
    private readonly string token = "token1";

    [Fact]
    public Task Should_generate_firebase_message()
    {
        var source = new MobilePushMessage
        {
            Body = "My Body",
            ConfirmText = "My Confirm",
            ConfirmUrl = "https://confirm.notifo.com",
            DeviceToken = token,
            DeviceType = MobileDeviceType.Android,
            Data = "data1",
            ImageLarge = "https://via.placeholder.com/600",
            ImageSmall = "https://via.placeholder.com/100",
            IsConfirmed = true,
            LinkText = "My Link",
            LinkUrl = "https://app.notifo.io",
            NotificationId = id,
            Silent = false,
            Subject = "My Subject",
            TrackDeliveredUrl = "https://track-delivered.notifo.com",
            TrackSeenUrl = "https://track-seen.notifo.com",
        };

        var message = source.ToFirebaseMessage(now);

        return Verify(message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_not_include_empty_fields_in_firebase_message(string? emptyText)
    {
        var source = new MobilePushMessage
        {
            Body = emptyText,
            ConfirmText = emptyText,
            ConfirmUrl = "https://confirm.notifo.com",
            DeviceToken = token,
            DeviceType = MobileDeviceType.Android,
            ImageLarge = emptyText,
            ImageSmall = emptyText,
            NotificationId = id,
            Subject = "My Subject",
            TrackDeliveredUrl = emptyText,
            TrackSeenUrl = emptyText
        };

        var message = source.ToFirebaseMessage(now);

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
        var source = new MobilePushMessage
        {
            DeviceToken = token,
            DeviceType = MobileDeviceType.Android,
            NotificationId = id,
            Wakeup = true
        };

        var message = source.ToFirebaseMessage(now);

        return Verify(message);
    }

    [Fact]
    public Task Should_include_time_to_live_property_if_set()
    {
        var timeToLive = 1000;

        var source = new MobilePushMessage
        {
            DeviceToken = token,
            DeviceType = MobileDeviceType.Android,
            NotificationId = id,
            TimeToLiveInSeconds = timeToLive
        };

        var message = source.ToFirebaseMessage(now);

        var timeToLiveSec = now.AddSeconds(1000).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

        Assert.Equal(timeToLiveSec, message.Apns.Headers["apns-expiration"]);
        Assert.Equal(timeToLive, message.Android.TimeToLive?.TotalSeconds);

        return Verify(message);
    }

    [Fact]
    public Task Should_set_time_to_live_to_zero_if_set_to_zero()
    {
        var timeToLive = 0;

        var source = new MobilePushMessage
        {
            DeviceToken = token,
            DeviceType = MobileDeviceType.Android,
            NotificationId = id,
            TimeToLiveInSeconds = 0
        };

        var message = source.ToFirebaseMessage(now);

        var timeToLiveSec = "0";

        Assert.Equal(timeToLiveSec, message.Apns.Headers["apns-expiration"]);
        Assert.Equal(timeToLive, message.Android.TimeToLive?.TotalSeconds);

        return Verify(message);
    }

    [Fact]
    public Task Should_not_include_time_to_live_property_if_not_set()
    {
        var source = new MobilePushMessage
        {
            DeviceToken = token,
            DeviceType = MobileDeviceType.Android,
            NotificationId = id
        };

        var message = source.ToFirebaseMessage(now);

        Assert.DoesNotContain("apns-expiration", message.Apns.Headers);
        Assert.Null(message.Android.TimeToLive);

        return Verify(message);
    }
}
