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

namespace TestSuite.ApiTests
{
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
            var userNotifications = await _.Notifo.CreateUserClient(user_0).Notifications.WaitForMyNotificationsAsyn(null, TimeSpan.FromSeconds(30));

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
            var userNotifications = await _.Notifo.CreateUserClient(user_0).Notifications.WaitForMyNotificationsAsyn(null, TimeSpan.FromSeconds(30));

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
            var notifications = await _.Client.Notifications.WaitForNotificationsAsync(_.AppId, user_0.Id, null, TimeSpan.FromSeconds(30));
            var notification = notifications.SingleOrDefault(x => x.Subject == subjectId);

            Assert.NotNull(notification);
            Assert.Null(notification.FirstSeen);


            // STEP 2: Mark as seen
            using (var httpClient = new HttpClient())
            {
                await httpClient.GetAsync(notification.TrackSeenUrl);
            }


            // Test if it has been marked as seen.
            notifications = (await _.Client.Notifications.GetNotificationsAsync(_.AppId, user_0.Id)).Items.ToArray();
            notification = notifications.SingleOrDefault(x => x.Subject == subjectId);

            Assert.NotNull(notification.FirstSeen);
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
            var notifications = await _.Client.Notifications.WaitForNotificationsAsync(_.AppId, user_0.Id, null, TimeSpan.FromSeconds(30));
            var notification = notifications.SingleOrDefault(x => x.Subject == subjectId);

            Assert.NotNull(notification);
            Assert.Null(notification.FirstConfirmed);


            // STEP 2: Mark as seen
            using (var httpClient = new HttpClient())
            {
                await httpClient.GetAsync(notification.ConfirmUrl);
            }


            // Test if it has been marked as seen.
            notifications = (await _.Client.Notifications.GetNotificationsAsync(_.AppId, user_0.Id)).Items.ToArray();
            notification = notifications.SingleOrDefault(x => x.Subject == subjectId);

            Assert.NotNull(notification.FirstConfirmed);
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
}
