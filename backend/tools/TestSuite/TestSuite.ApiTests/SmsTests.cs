// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;
using TestSuite.Utils;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class SmsTests : IClassFixture<ClientFixture>
{
    private static readonly string PhoneNumber = "00436703091161";
    private static readonly string AccessKey = TestHelpers.GetAndPrintValue("messagebird:accessKey", "invalid");

    public ClientFixture _ { get; }

    public SmsTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_send_sms_with_template()
    {
        // In pull requests from forks we cannot inject the secret key.
        if (string.IsNullOrWhiteSpace(AccessKey))
        {
            return;
        }

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

        var template_0 = await _.Client.SmsTemplates.PostTemplateAsync(app_0.Id, smsTemplateRequest);

        var smsTemplate = new SmsTemplateDto
        {
            Text = "<start>{{ notification.subject }}</end>"
        };

        await _.Client.SmsTemplates.PutTemplateLanguageAsync(app_0.Id, template_0.Id, "en", smsTemplate);


        // STEP 2: Create integration
        var emailIntegrationRequest = new CreateIntegrationDto
        {
            Type = "MessageBird",
            Properties = new Dictionary<string, string>
            {
                ["accessKey"] = AccessKey,
                ["phoneNumber"] = PhoneNumber,
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
                    PhoneNumber = PhoneNumber
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
        var messageBird = new MessageBirdClient(AccessKey);

        var text = $"<start>{subjectId}</end>";

        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
        {
            while (!cts.IsCancellationRequested)
            {
                var messages = await messageBird.GetMessagesAsync(200);

                if (messages.Items.Any(x => x.Body == text && x.Recipients.Items[0].Status == "delivered"))
                {
                    return;
                }

                await Task.Delay(1000);
            }
        }

        Assert.False(true, "SMS not sent.");
    }

    [Fact]
    public async Task Should_send_sms_without_template()
    {
        // In pull requests from forks we cannot inject the secret key.
        if (string.IsNullOrWhiteSpace(AccessKey))
        {
            return;
        }

        var appName = Guid.NewGuid().ToString();

        // STEP 0: Create app
        var createRequest = new UpsertAppDto
        {
            Name = appName
        };

        var app_0 = await _.Client.Apps.PostAppAsync(createRequest);


        // STEP 1: Create integration
        var emailIntegrationRequest = new CreateIntegrationDto
        {
            Type = "MessageBird",
            Properties = new Dictionary<string, string>
            {
                ["accessKey"] = AccessKey,
                ["phoneNumber"] = PhoneNumber,
                ["phoneNumbers"] = string.Empty
            },
            Enabled = true
        };

        await _.Client.Apps.PostIntegrationAsync(app_0.Id, emailIntegrationRequest);


        // STEP 2: Create user
        var userRequest = new UpsertUsersDto
        {
            Requests = new List<UpsertUserDto>
            {
                new UpsertUserDto
                {
                    PhoneNumber = PhoneNumber
                }
            }
        };

        var users_0 = await _.Client.Users.PostUsersAsync(app_0.Id, userRequest);
        var user_0 = users_0.First();


        // STEP 3: Send SMS
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
        var messageBird = new MessageBirdClient(AccessKey);

        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
        {
            while (!cts.IsCancellationRequested)
            {
                var messages = await messageBird.GetMessagesAsync(200);

                if (messages.Items.Any(x => x.Body == subjectId && x.Recipients.Items[0].Status == "delivered"))
                {
                    return;
                }

                await Task.Delay(1000);
            }
        }

        Assert.False(true, "SMS not sent.");
    }
}
