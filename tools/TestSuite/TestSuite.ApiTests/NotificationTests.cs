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

[UsesVerify]
public partial class NotificationTests : IClassFixture<CreatedAppFixture>
{
    private readonly string subject = Guid.NewGuid().ToString();

    public CreatedAppFixture _ { get; set; }

    public NotificationTests(CreatedAppFixture fixture)
    {
        VerifierSettings.DontScrubDateTimes();

        _ = fixture;
    }

    [Fact]
    public async Task Should_send_self_formatted_notifications()
    {
        // STEP 0: Create user.
        var user_0 = await CreateUserAsync();


        // STEP 1: Send Notification
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
                        }
                    }
                }
            }
        };

        await _.Client.Events.PostEventsAsync(_.AppId, publishRequest);


        // Test that notification has been created.
        var notifications = await _.Client.Notifications.PollAsync(_.AppId, user_0.Id, null);

        Assert.Contains(notifications, x => x.Subject == subject);


        // Test that user notifications have been created.
        var userNotifications = await _.Notifo.CreateUserClient(user_0).Notifications.PollMyAsync(null);

        Assert.Contains(userNotifications, x => x.Subject == subject);

        await Verify(userNotifications)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMember<UserNotificationBaseDto>(x => x.TrackingToken)
            .IgnoreMember<UserNotificationBaseDto>(x => x.TrackDeliveredUrl)
            .IgnoreMember<UserNotificationBaseDto>(x => x.TrackSeenUrl);
    }

    [Fact]
    public async Task Should_query_notifications_by_correlation_id()
    {
        var correlationId = Guid.NewGuid().ToString();

        // STEP 0: Create users.
        var user_0 = await CreateUserAsync();
        var user_1 = await CreateUserAsync();


        // STEP 1: Send Notification
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
                        }
                    },
                    CorrelationId = correlationId
                },
                new PublishDto
                {
                    Topic = $"users/{user_1.Id}",
                    Preformatted = new NotificationFormattingDto
                    {
                        Subject = new LocalizedText
                        {
                            ["en"] = subject
                        }
                    },
                    CorrelationId = correlationId
                }
            }
        };

        await _.Client.Events.PostEventsAsync(_.AppId, publishRequest);


        // Test that notification has been created.
        var args = new PollingArguments<UserNotificationDetailsDto>
        {
            ExpectedCount = 2
        };

        var notifications = await _.Client.Notifications.PollCorrelatedAsync(_.AppId, correlationId, args);

        Assert.Equal(2, notifications.Count(x => x.CorrelationId == correlationId));
    }

    [Fact]
    public async Task Should_send_template_formatted_notifications()
    {
        // STEP 0: Create user.
        var user_0 = await CreateUserAsync();


        // STEP 1: Create template.
        var templateCode = Guid.NewGuid().ToString();

        var templateRequest = new UpsertTemplatesDto
        {
            Requests = new List<UpsertTemplateDto>
            {
                new UpsertTemplateDto
                {
                    Code = templateCode,
                    Formatting = new NotificationFormattingDto
                    {
                        Subject = new LocalizedText
                        {
                            ["en"] = subject
                        }
                    }
                }
            }
        };

        await _.Client.Templates.PostTemplatesAsync(_.AppId, templateRequest);


        // STEP 2: Send Notification
        var publishRequest = new PublishManyDto
        {
            Requests = new List<PublishDto>
            {
                new PublishDto
                {
                    Topic = $"users/{user_0.Id}",
                    TemplateCode = templateCode
                }
            }
        };

        await _.Client.Events.PostEventsAsync(_.AppId, publishRequest);


        // Test that notification has been created.
        var notifications = await _.Client.Notifications.PollAsync(_.AppId, user_0.Id, null);

        Assert.Contains(notifications, x => x.Subject == subject);


        // Test that user notifications have been created.
        var userNotifications = await _.Notifo.CreateUserClient(user_0).Notifications.PollMyAsync(null);

        Assert.Contains(userNotifications, x => x.Subject == subject);

        await Verify(userNotifications)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMember<UserNotificationBaseDto>(x => x.TrackingToken)
            .IgnoreMember<UserNotificationBaseDto>(x => x.TrackDeliveredUrl)
            .IgnoreMember<UserNotificationBaseDto>(x => x.TrackSeenUrl);
    }

    [Theory]
    [InlineData(TrackingStrategy.TrackingToken)]
    [InlineData(TrackingStrategy.TrackingUrl)]
    [InlineData(TrackingStrategy.TrackingUrlWithSDKClient)]
    [InlineData(TrackingStrategy.Id)]
    public async Task Should_mark_notification_as_confirmed(TrackingStrategy strategy)
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
        await MarkAsConfirmedAsync(notification_0, user_0, strategy);

        // Test if it has been marked as seen.
        var args1 = new PollingArguments<UserNotificationDetailsDto>
        {
            Condition = x => x.Subject == subject && x.FirstConfirmed != null
        };

        var notifications_1 = await _.Client.Notifications.PollAsync(_.AppId, user_0.Id, args1);
        var notification_1 = notifications_1.SingleOrDefault();

        Assert.NotNull(notification_1.FirstConfirmed);
        Assert.NotNull(notification_1.FirstSeen);
        Assert.NotNull(notification_1.FirstDelivered);
    }

    [Theory]
    [InlineData(TrackingStrategy.TrackingToken)]
    [InlineData(TrackingStrategy.TrackingUrl)]
    [InlineData(TrackingStrategy.TrackingUrlWithSDKClient)]
    [InlineData(TrackingStrategy.Id)]
    public async Task Should_mark_notification_as_seen(TrackingStrategy strategy)
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
        await MarkAsSeenAsync(notification_0, user_0, strategy);

        // Test if it has been marked as seen.
        var args1 = new PollingArguments<UserNotificationDetailsDto>
        {
            Condition = x => x.Subject == subject && x.FirstSeen != null
        };

        var notifications_1 = await _.Client.Notifications.PollAsync(_.AppId, user_0.Id, args1);
        var notification_1 = notifications_1.SingleOrDefault();

        Assert.NotNull(notification_1.FirstSeen);
        Assert.NotNull(notification_1.FirstDelivered);
    }

    [Theory]
    [InlineData(TrackingStrategy.TrackingUrl)]
    [InlineData(TrackingStrategy.TrackingUrlWithSDKClient)]
    public async Task Should_mark_notification_as_delivered(TrackingStrategy strategy)
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


        // STEP 2: Mark as delivered
        await MarkAsDeliveredAsync(notification_0, strategy);

        // Test if it has been marked as delivered.
        var args1 = new PollingArguments<UserNotificationDetailsDto>
        {
            Condition = x => x.Subject == subject && x.FirstDelivered != null
        };

        var notifications_1 = await _.Client.Notifications.PollAsync(_.AppId, user_0.Id, args1);
        var notification_1 = notifications_1.SingleOrDefault();

        Assert.NotNull(notification_1.FirstDelivered);
    }

    private async Task MarkAsSeenAsync(UserNotificationDetailsDto notification, UserDto user, TrackingStrategy strategy)
    {
        switch (strategy)
        {
            case TrackingStrategy.TrackingUrl:
                using (var httpClient = new HttpClient())
                {
                    await httpClient.GetAsync(notification.TrackSeenUrl);
                }

                break;

            case TrackingStrategy.TrackingUrlWithSDKClient:
                using (var httpClient = _.Client.CreateHttpClient())
                {
                    await httpClient.GetAsync(notification.TrackSeenUrl);
                }

                break;

            case TrackingStrategy.TrackingToken:
                var tokenRequest = new TrackNotificationDto
                {
                    Seen = new List<string>
                    {
                        notification.TrackingToken
                    }
                };

                await _.BuildUserClient(user).Notifications.ConfirmMeAsync(tokenRequest);
                break;

            case TrackingStrategy.Id:
                var idRequest = new TrackNotificationDto
                {
                    Seen = new List<string>
                    {
                        notification.Id.ToString()
                    }
                };

                await _.BuildUserClient(user).Notifications.ConfirmMeAsync(idRequest);
                break;

            default:
                throw new NotSupportedException();
        }
    }

    private async Task MarkAsConfirmedAsync(UserNotificationDetailsDto notification, UserDto user, TrackingStrategy strategy)
    {
        switch (strategy)
        {
            case TrackingStrategy.TrackingUrl:
                using (var httpClient = new HttpClient())
                {
                    await httpClient.GetAsync(notification.ConfirmUrl);
                }

                break;

            case TrackingStrategy.TrackingToken:
                var tokenRequest = new TrackNotificationDto
                {
                    Confirmed = notification.TrackingToken
                };

                await _.BuildUserClient(user).Notifications.ConfirmMeAsync(tokenRequest);
                break;

            case TrackingStrategy.Id:
                var idRequest = new TrackNotificationDto
                {
                    Confirmed = notification.Id.ToString()
                };

                await _.BuildUserClient(user).Notifications.ConfirmMeAsync(idRequest);
                break;

            default:
                throw new NotSupportedException();
        }
    }

    private static async Task MarkAsDeliveredAsync(UserNotificationDetailsDto notification, TrackingStrategy strategy)
    {
        switch (strategy)
        {
            case TrackingStrategy.TrackingUrl:
                using (var httpClient = new HttpClient())
                {
                    await httpClient.GetAsync(notification.TrackDeliveredUrl);
                }

                break;
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
                    }
                }
            }
        };

        await _.Client.Events.PostEventsAsync(_.AppId, publishRequest);
    }
}
