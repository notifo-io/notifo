// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MessageBird;
using MessageBird.Objects;
using Notifo.SDK;
using TestSuite.Fixtures;
using TestSuite.Utils;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class SmsTests : IClassFixture<ClientFixture>
    {
        public ClientFixture _ { get; }

        public SmsTests(ClientFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_send_SMS()
        {
            var phoneNumber = "00436703091161";

            var accessKey = TestHelpers.GetAndPrintValue("messagebird:accessKey", "invalid");


            var appName = Guid.NewGuid().ToString();

            // STEP 0: Create app
            var createRequest = new UpsertAppDto
            {
                Name = appName
            };

            var app_0 = await _.Client.Apps.PostAppAsync(createRequest);


            // STEP 1: Create sms template.
            var smsTemplateRequest = new CreateChannelTemplateDto
            {
            };

            await _.Client.SmsTemplates.PostTemplateAsync(app_0.Id, smsTemplateRequest);


            // STEP 2: Create integration
            var emailIntegrationRequest = new CreateIntegrationDto
            {
                Type = "MessageBird",
                Properties = new Dictionary<string, string>
                {
                    ["accessKey"] = accessKey,
                    ["phoneNumber"] = phoneNumber,
                    ["phoneNumbers"] = string.Empty
                },
                Enabled = true
            };

            await _.Client.Apps.PostIntegrationAsync(app_0.Id, emailIntegrationRequest);


            // STEP 3: Create user
            var userRequest = new UpsertUsersDto
            {
                Requests = new List<UpsertUserDto>
                {
                    new UpsertUserDto
                    {
                        PhoneNumber = phoneNumber
                    }
                }
            };

            var users_0 = await _.Client.Users.PostUsersAsync(app_0.Id, userRequest);
            var user_0 = users_0.First();


            // STEP 4: Send SMS
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
                            [Providers.Sms] = new ChannelSettingDto
                            {
                                Send = ChannelSend.Send
                            }
                        }
                    }
                }
            };

            await _.Client.Events.PostEventsAsync(app_0.Id, publishRequest);


            // Get SMS status
            var messageBird = Client.CreateDefault(accessKey);

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
            {
                while (!cts.IsCancellationRequested)
                {
                    var messages = messageBird.ListMessages(string.Empty, 200);

                    if (messages.Items.Any(x => x.Body.Contains(subjectId, StringComparison.OrdinalIgnoreCase) && x.Recipients.Items[0].Status == Recipient.RecipientStatus.Delivered))
                    {
                        return;
                    }

                    await Task.Delay(1000);
                }
            }

            Assert.False(true, "SMS not sent.");
        }
    }
}
