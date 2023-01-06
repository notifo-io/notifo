// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class NotificationMobileTests : IClassFixture<CreatedAppFixture>
{
    private readonly string deviceIdentifier = Guid.NewGuid().ToString();
    private readonly string token = Guid.NewGuid().ToString();
    private readonly string subject = Guid.NewGuid().ToString();

    public CreatedAppFixture _ { get; set; }

    public NotificationMobileTests(CreatedAppFixture fixture)
    {
        VerifierSettings.DontScrubDateTimes();

        _ = fixture;
    }

    [Theory]
    [InlineData(TrackingStrategy.TrackingToken, true)]
    [InlineData(TrackingStrategy.TrackingToken, false)]
    [InlineData(TrackingStrategy.Id, true)]
    [InlineData(TrackingStrategy.Id, false)]
    public async Task Should_mark_notification_as_confirmed(TrackingStrategy strategy, bool useDeviceIdentifier)
    {
        // STEP 0: Create user.
        var user_0 = await CreateUserAsync();


        // STEP 1: Send Notification
        await CreateNotificationAsync(user_0);


        // Test that notification has been created.
        var args0 = new PollingArguments<UserNotificationDetailsDto>
        {
            Condition = x => x.Subject == subject
        };

        var notifications_0 = await _.Client.Notifications.PollAsync(_.AppId, user_0.Id, args0);
        var notification_0 = notifications_0.SingleOrDefault();

        Assert.NotNull(notification_0);
        Assert.Null(notification_0.FirstConfirmed);


        // STEP 2: Mark as confirmed
        await MarkAsConfirmedAsync(notification_0, user_0, strategy, useDeviceIdentifier);

        // Test if it has been marked as seen.
        var args1 = new PollingArguments<UserNotificationDetailsDto>
        {
            Condition = x => x.Subject == subject && x.FirstConfirmed != null
        };

        var notifications_1 = await _.Client.Notifications.PollAsync(_.AppId, user_0.Id, args1);
        var notification_1 = notifications_1.SingleOrDefault();

        Assert.NotNull(notification_1?.FirstConfirmed);
        Assert.NotNull(notification_1?.FirstSeen);
        Assert.NotNull(notification_1?.FirstDelivered);
    }

    [Theory]
    [InlineData(TrackingStrategy.TrackingToken, true)]
    [InlineData(TrackingStrategy.TrackingToken, false)]
    [InlineData(TrackingStrategy.Id, true)]
    [InlineData(TrackingStrategy.Id, false)]
    public async Task Should_mark_notification_as_seen(TrackingStrategy strategy, bool useDeviceIdentifier)
    {
        // STEP 0: Create user.
        var user_0 = await CreateUserAsync();


        // STEP 1: Send Notification
        await CreateNotificationAsync(user_0);


        // Test that notification has been created.
        var args0 = new PollingArguments<UserNotificationDetailsDto>
        {
            Condition = x => x.Subject == subject
        };

        var notifications_0 = await _.Client.Notifications.PollAsync(_.AppId, user_0.Id, args0);
        var notification_0 = notifications_0.SingleOrDefault();

        Assert.NotNull(notification_0);
        Assert.Null(notification_0.FirstSeen);


        // STEP 2: Mark as seen
        await MarkAsSeenAsync(notification_0, user_0, strategy, useDeviceIdentifier);

        // Test if it has been marked as seen.
        var args1 = new PollingArguments<UserNotificationDetailsDto>
        {
            Condition = x => x.Subject == subject && x.FirstSeen != null
        };

        var notifications_1 = await _.Client.Notifications.PollAsync(_.AppId, user_0.Id, args1);
        var notification_1 = notifications_1.SingleOrDefault();

        Assert.NotNull(notification_1?.FirstSeen);
        Assert.NotNull(notification_1?.FirstDelivered);
        Assert.NotNull(notification_1?.Channels[Providers.MobilePush].FirstSeen);
        Assert.NotNull(notification_1?.Channels[Providers.MobilePush].FirstDelivered);
        Assert.NotNull(notification_1?.Channels[Providers.MobilePush].Status.First().Value.FirstSeen);
        Assert.NotNull(notification_1?.Channels[Providers.MobilePush].Status.First().Value.FirstDelivered);
    }

    private async Task MarkAsSeenAsync(UserNotificationDetailsDto notification, UserDto user, TrackingStrategy strategy, bool useDeviceIdentifier)
    {
        switch (strategy)
        {
            case TrackingStrategy.TrackingToken:
                var tokenRequest = new TrackNotificationDto
                {
                    Channel = Providers.MobilePush,
                    Seen = new List<string>
                    {
                        notification.TrackingToken
                    },
                    DeviceIdentifier = useDeviceIdentifier ? deviceIdentifier : token
                };

                await _.BuildUserClient(user).Notifications.ConfirmMeAsync(tokenRequest);
                break;

            case TrackingStrategy.Id:
                var idRequest = new TrackNotificationDto
                {
                    Channel = Providers.MobilePush,
                    Seen = new List<string>
                    {
                        notification.Id.ToString()
                    },
                    DeviceIdentifier = useDeviceIdentifier ? deviceIdentifier : token
                };

                await _.BuildUserClient(user).Notifications.ConfirmMeAsync(idRequest);
                break;

            default:
                throw new NotSupportedException();
        }
    }

    private async Task MarkAsConfirmedAsync(UserNotificationDetailsDto notification, UserDto user, TrackingStrategy strategy, bool useDeviceIdentifier)
    {
        switch (strategy)
        {
            case TrackingStrategy.TrackingToken:
                var tokenRequest = new TrackNotificationDto
                {
                    Channel = Providers.MobilePush,
                    Confirmed = notification.TrackingToken,
                    DeviceIdentifier = useDeviceIdentifier ? deviceIdentifier : token
                };

                await _.BuildUserClient(user).Notifications.ConfirmMeAsync(tokenRequest);
                break;

            case TrackingStrategy.Id:
                var idRequest = new TrackNotificationDto
                {
                    Channel = Providers.MobilePush,
                    Confirmed = notification.Id.ToString(),
                    DeviceIdentifier = useDeviceIdentifier ? deviceIdentifier : token
                };

                await _.BuildUserClient(user).Notifications.ConfirmMeAsync(idRequest);
                break;

            default:
                throw new NotSupportedException();
        }
    }

    private async Task<UserDto> CreateUserAsync()
    {
        var userRequest = new UpsertUsersDto
        {
            Requests = new List<UpsertUserDto>
            {
                new UpsertUserDto()
            }
        };

        var users_0 = await _.Client.Users.PostUsersAsync(_.AppId, userRequest);
        var user_0 = users_0.First();

        await _.BuildUserClient(user_0)
            .MobilePush.PostMyTokenAsync(new RegisterMobileTokenDto
            {
                Token = token,
                DeviceIdentifier = deviceIdentifier,
                DeviceType = MobileDeviceType.IOS
            });

        return user_0;
    }

    private async Task CreateNotificationAsync(UserDto user_0)
    {
        var publishRequest = new PublishManyDto
        {
            Requests = new List<PublishDto>
            {
                new PublishDto
                {
                    Topic = $"users/{user_0.Id}",
                    Preformatted = new NotificationFormattingDto
                    {
                        Subject = new LocalizedText
                        {
                            ["en"] = subject
                        },
                        ConfirmMode = ConfirmMode.Explicit
                    },
                    Settings = new Dictionary<string, ChannelSettingDto>
                    {
                        [Providers.MobilePush] = new ChannelSettingDto
                        {
                            Send = ChannelSend.Send
                        }
                    }
                }
            }
        };

        await _.Client.Events.PostEventsAsync(_.AppId, publishRequest);
    }
}
