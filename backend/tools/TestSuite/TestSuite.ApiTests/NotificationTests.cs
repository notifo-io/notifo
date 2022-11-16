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
public class NotificationTests : IClassFixture<CreatedAppFixture>
{
    private readonly string subjectId = Guid.NewGuid().ToString();

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
                            ["en"] = subjectId
                        }
                    }
                }
            }
        };

        await _.Client.Events.PostEventsAsync(_.AppId, publishRequest);


        // Test that notification has been created.
        var notifications = await _.Client.Notifications.WaitForNotificationsAsync(_.AppId, user_0.Id, null, TimeSpan.FromSeconds(30));

        Assert.Contains(notifications, x => x.Subject == subjectId);


        // Test that user notifications have been created.
        var userNotifications = await _.Notifo.CreateUserClient(user_0).Notifications.WaitForMyNotificationsAsync(null, TimeSpan.FromSeconds(30));

        Assert.Contains(userNotifications, x => x.Subject == subjectId);

        await Verify(userNotifications)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMember<UserNotificationBaseDto>(x => x.TrackingToken)
            .IgnoreMember<UserNotificationBaseDto>(x => x.TrackDeliveredUrl)
            .IgnoreMember<UserNotificationBaseDto>(x => x.TrackSeenUrl);
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
                            ["en"] = subjectId
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
        var notifications = await _.Client.Notifications.WaitForNotificationsAsync(_.AppId, user_0.Id, null, TimeSpan.FromSeconds(30));

        Assert.Contains(notifications, x => x.Subject == subjectId);


        // Test that user notifications have been created.
        var userNotifications = await _.Notifo.CreateUserClient(user_0).Notifications.WaitForMyNotificationsAsync(null, TimeSpan.FromSeconds(30));

        Assert.Contains(userNotifications, x => x.Subject == subjectId);

        await Verify(userNotifications)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMember<UserNotificationBaseDto>(x => x.TrackingToken)
            .IgnoreMember<UserNotificationBaseDto>(x => x.TrackDeliveredUrl)
            .IgnoreMember<UserNotificationBaseDto>(x => x.TrackSeenUrl);
    }

    [Fact]
    public async Task Should_mark_notification_as_seen()
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
                            ["en"] = subjectId
                        }
                    }
                }
            }
        };

        await _.Client.Events.PostEventsAsync(_.AppId, publishRequest);


        // Test that notification has been created.
        var predicate_0 = new Func<UserNotificationDetailsDto, bool>(x => x.Subject == subjectId);

        var notifications_0 = await _.Client.Notifications.WaitForNotificationsAsync(_.AppId, user_0.Id, predicate_0, TimeSpan.FromSeconds(30));
        var notification_0 = notifications_0.SingleOrDefault();

        Assert.NotNull(notification_0);
        Assert.Null(notification_0.FirstSeen);


        // STEP 2: Mark as seen
        using (var httpClient = new HttpClient())
        {
            await httpClient.GetAsync(notification_0.TrackSeenUrl);
        }

        // Test if it has been marked as seen.
        var predicate_1 = new Func<UserNotificationDetailsDto, bool>(x => x.Subject == subjectId && x.FirstSeen != null);

        var notifications_1 = await _.Client.Notifications.WaitForNotificationsAsync(_.AppId, user_0.Id, predicate_1, TimeSpan.FromSeconds(30));
        var notification_1 = notifications_1.SingleOrDefault();

        Assert.NotNull(notification_1.FirstSeen);
    }

    [Fact]
    public async Task Should_mark_notification_as_confirmed_explicitely()
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
                            ["en"] = subjectId
                        },
                        ConfirmMode = ConfirmMode.Explicit
                    }
                }
            }
        };

        await _.Client.Events.PostEventsAsync(_.AppId, publishRequest);


        // Test that notification has been created.
        var predicate_0 = new Func<UserNotificationDetailsDto, bool>(x => x.Subject == subjectId);

        var notifications_0 = await _.Client.Notifications.WaitForNotificationsAsync(_.AppId, user_0.Id, predicate_0, TimeSpan.FromSeconds(30));
        var notification_0 = notifications_0.SingleOrDefault();

        Assert.NotNull(notification_0);
        Assert.Null(notification_0.FirstConfirmed);


        // STEP 2: Mark as seen
        using (var httpClient = new HttpClient())
        {
            await httpClient.GetAsync(notification_0.ConfirmUrl);
        }

        // Test if it has been marked as seen.
        var predicate_1 = new Func<UserNotificationDetailsDto, bool>(x => x.Subject == subjectId && x.FirstConfirmed != null);

        var notifications_1 = await _.Client.Notifications.WaitForNotificationsAsync(_.AppId, user_0.Id, predicate_1, TimeSpan.FromSeconds(30));
        var notification_1 = notifications_1.SingleOrDefault();

        Assert.NotNull(notification_1.FirstSeen);
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
}
