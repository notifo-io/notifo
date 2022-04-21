// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;
using TestSuite.Utils;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class WebhookTests : IClassFixture<ClientFixture>, IClassFixture<WebhookCatcherFixture>
    {
        private readonly WebhookCatcherClient webhookCatcher;

        public ClientFixture _ { get; }

        public WebhookTests(ClientFixture fixture, WebhookCatcherFixture webhookCatcher)
        {
            _ = fixture;

            this.webhookCatcher = webhookCatcher.Client;
        }

        [Fact]
        public async Task Should_send_webhook()
        {
            var appName = Guid.NewGuid().ToString();

            // STEP 0: Create app
            var createRequest = new UpsertAppDto
            {
                Name = appName
            };

            var app_0 = await _.Client.Apps.PostAppAsync(createRequest);


            // STEP 1: Start webhook session
            var (url, sessionId) = await webhookCatcher.CreateSessionAsync();


            // STEP 2: Create integration
            var emailIntegrationRequest = new CreateIntegrationDto
            {
                Type = "Webhook",
                Properties = new Dictionary<string, string>
                {
                    ["Url"] = url,
                    ["SendAlways"] = "true",
                    ["SendConfirm"] = "true"
                },
                Enabled = true
            };

            await _.Client.Apps.PostIntegrationAsync(app_0.Id, emailIntegrationRequest);


            // STEP 3: Create user
            var userRequest = new UpsertUsersDto
            {
                Requests = new List<UpsertUserDto>
                {
                    new UpsertUserDto()
                }
            };

            var users_0 = await _.Client.Users.PostUsersAsync(app_0.Id, userRequest);
            var user_0 = users_0.First();


            // STEP 4: Send email
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
                        },
                        Settings = new Dictionary<string, ChannelSettingDto>
                        {
                            [Providers.WebHook] = new ChannelSettingDto
                            {
                                Send = ChannelSend.Send
                            }
                        }
                    }
                }
            };

            await _.Client.Events.PostEventsAsync(app_0.Id, publishRequest);


            var requests = await webhookCatcher.WaitForRequestsAsync(sessionId, TimeSpan.FromSeconds(30));

            Assert.Contains(requests, x => x.Method == "POST" && x.Content.Contains(subjectId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
