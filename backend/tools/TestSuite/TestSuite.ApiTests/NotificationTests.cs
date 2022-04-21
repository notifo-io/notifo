// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class NotificationTests : IClassFixture<CreatedAppFixture>
    {
        public CreatedAppFixture _ { get; set; }

        public NotificationTests(CreatedAppFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_send_self_formatted_notifications()
        {
            // STEP 0: Create user.
            var userRequest = new UpsertUsersDto
            {
                Requests = new List<UpsertUserDto>
                {
                    new UpsertUserDto()
                }
            };

            var users_0 = await _.Client.Users.PostUsersAsync(_.AppId, userRequest);
            var user_0 = users_0.First();


            // STEP 1: Send Notification
            var subjectId = Guid.NewGuid().ToString();

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
        }

        [Fact]
        public async Task Should_send_template_formatted_notifications()
        {
            var subjectId = Guid.NewGuid().ToString();

            // STEP 0: Create user.
            var userRequest = new UpsertUsersDto
            {
                Requests = new List<UpsertUserDto>
                {
                    new UpsertUserDto()
                }
            };

            var users_0 = await _.Client.Users.PostUsersAsync(_.AppId, userRequest);
            var user_0 = users_0.First();


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
        }
    }
}
