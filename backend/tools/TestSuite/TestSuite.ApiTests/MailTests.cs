// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Notifo.SDK;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class MailTests : IClassFixture<ClientFixture>, IClassFixture<MailcatcherFixture>
{
    private readonly MailcatcherClient mailcatcher;

    public ClientFixture _ { get; }

    public MailTests(ClientFixture fixture, MailcatcherFixture mailcatcher)
    {
        _ = fixture;

        this.mailcatcher = mailcatcher.Client;
    }

    [Theory]
    [InlineData("Default")]
    [InlineData("Liquid")]
    public async Task Should_send_email(string kind)
    {
        var appName = Guid.NewGuid().ToString();

        // STEP 0: Create app
        var createRequest = new UpsertAppDto
        {
            Name = appName
        };

        var app_0 = await _.Client.Apps.PostAppAsync(createRequest);


        // STEP 1: Create email template.
        var emailTemplateRequest = new CreateChannelTemplateDto
        {
            Kind = kind
        };

        await _.Client.EmailTemplates.PostTemplateAsync(app_0.Id, emailTemplateRequest);


        // STEP 2: Create integration
        var emailIntegrationRequest = new CreateIntegrationDto
        {
            Type = "SMTP",
            Properties = new Dictionary<string, string>
            {
                ["host"] = mailcatcher.SmtpHost,
                ["fromEmail"] = "hello@notifo.io",
                ["fromName"] = "Hello Notifo",
                ["port"] = mailcatcher.SmtpPort.ToString(CultureInfo.InvariantCulture)
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
                    EmailAddress = "hello@notifo.io"
                }
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
                        [Providers.Email] = new ChannelSettingDto
                        {
                            Send = ChannelSend.Send
                        }
                    }
                }
            }
        };

        await _.Client.Events.PostEventsAsync(app_0.Id, publishRequest);


        // Get email status
        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
        {
            while (!cts.IsCancellationRequested)
            {
                var messages = await mailcatcher.GetMessagesAsync(cts.Token);
                var message = messages.FirstOrDefault(x => x.Subject.Contains(appName, StringComparison.OrdinalIgnoreCase));

                if (message != null)
                {
                    var body = await mailcatcher.GetBodyAsync(message.Id, cts.Token);

                    Assert.Contains(subjectId, body.Plain, StringComparison.OrdinalIgnoreCase);
                    Assert.Contains(subjectId, body.Html, StringComparison.OrdinalIgnoreCase);
                    return;
                }

                await Task.Delay(50);
            }
        }

        Assert.False(true, "Email not received.");
    }

    [Fact]
    public async Task Should_not_send_email_when_template_missing()
    {
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
            Type = "SMTP",
            Properties = new Dictionary<string, string>
            {
                ["host"] = mailcatcher.SmtpHost,
                ["fromEmail"] = "hello@notifo.io",
                ["fromName"] = "Hello Notifo",
                ["port"] = mailcatcher.SmtpPort.ToString(CultureInfo.InvariantCulture)
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
                    EmailAddress = "hello@notifo.io"
                }
            }
        };

        var users_0 = await _.Client.Users.PostUsersAsync(app_0.Id, userRequest);
        var user_0 = users_0.First();


        // STEP 3: Send email
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
                        [Providers.Email] = new ChannelSettingDto
                        {
                            Send = ChannelSend.Send
                        }
                    }
                }
            }
        };

        await _.Client.Events.PostEventsAsync(app_0.Id, publishRequest);


        // STEP 4: Wait for log entries.
        var logs = await _.Client.Logs.PollAsync(app_0.Id, null, TimeSpan.FromSeconds(30));

        Assert.Contains(logs, x => x.Message.Contains("Cannot find template", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("Default")]
    [InlineData("Liquid")]
    public async Task Should_render_email_preview(string kind)
    {
        var appName = Guid.NewGuid().ToString();

        // STEP 0: Create app
        var createRequest = new UpsertAppDto
        {
            Name = appName
        };

        var app_0 = await _.Client.Apps.PostAppAsync(createRequest);


        // STEP 1: Create email template.
        var emailTemplateRequest = new CreateChannelTemplateDto
        {
            Kind = kind
        };

        var template_0 = await _.Client.EmailTemplates.PostTemplateAsync(app_0.Id, emailTemplateRequest);


        // STEP 2: Render preview
        var previewRequest = new EmailPreviewRequestDto
        {
            Kind = kind,
            Template = template_0.Languages.First().Value.BodyHtml
        };

        var preview_0 = await _.Client.EmailTemplates.PostPreviewAsync(app_0.Id, previewRequest);

        Assert.NotNull(preview_0.Result);
        Assert.NotNull(preview_0.Errors);
        Assert.Empty(preview_0.Errors);
    }

    [Theory]
    [InlineData("Default")]
    [InlineData("Liquid")]
    public async Task Should_not_render_invalid_email_preview(string kind)
    {
        var appName = Guid.NewGuid().ToString();

        // STEP 0: Create app
        var createRequest = new UpsertAppDto
        {
            Name = appName
        };

        var app_0 = await _.Client.Apps.PostAppAsync(createRequest);


        // STEP 1: Render preview
        var previewRequest = new EmailPreviewRequestDto
        {
            Kind = kind,
            Template = "invalid"
        };

        var preview_0 = await _.Client.EmailTemplates.PostPreviewAsync(app_0.Id, previewRequest);

        Assert.Null(preview_0.Result);
        Assert.NotNull(preview_0.Errors);
        Assert.NotEmpty(preview_0.Errors);
    }
}
