// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Email.Formatting;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Utils;

#pragma warning disable MA0056 // Do not call overridable members in constructor

namespace Notifo.Domain.Channels.Email;

public abstract class EmailTemplateTestsBase
{
    private readonly EmailTemplate templateDefault;
    private readonly EmailTemplate templateCustom;
    private readonly IEmailFormatter emailFormatter;

    protected abstract string Name { get; }

    protected EmailTemplateTestsBase()
    {
        var emailUrl = A.Fake<IEmailUrl>();

        A.CallTo(() => emailUrl.EmailPreferences(A<string>._, A<string>._))
            .Returns("url/to/email-preferences");

        var imageFormatter = A.Fake<IImageFormatter>();

        A.CallTo(() => imageFormatter.AddPreset(A<string>._, A<string>._))
            .ReturnsLazily(x => x.GetArgument<string>(0));

        emailFormatter = CreateFormatter(emailUrl, imageFormatter);

        templateDefault = emailFormatter.CreateInitialAsync().AsTask().Result;
        templateCustom = ParseCustomAsync().Result;
    }

    protected abstract IEmailFormatter CreateFormatter(IEmailUrl url, IImageFormatter imageFormatter);

    [Fact]
    public async Task Should_generate_simple_template()
    {
        var jobs = ToJobs(
            new UserNotification
            {
                UserLanguage = "en",
                Formatting = new NotificationFormatting<string>
                {
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    Subject = "subject1",
                    ImageSmall = string.Empty,
                    ImageLarge = string.Empty
                }
            });

        var (html, text) = await FormatAsync(jobs);

        await WriteResultFileAsync("template-single.html", html);
        await WriteResultFileAsync("template-single.txt", text);

        foreach (var notification in jobs.Select(x => x.Notification))
        {
            // Test Html result.
            Assert.Contains(notification.Formatting.Body!, html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(notification.Formatting.Subject, html, StringComparison.OrdinalIgnoreCase);

            // Test Text result.
            Assert.Contains(notification.Formatting.Body!, text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(notification.Formatting.Subject, text, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("url/to/email-preferences", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("url/to/email-preferences", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_generate_template_with_link()
    {
        var jobs = ToJobs(
                new UserNotification
                {
                    UserLanguage = "en",
                    Formatting = new NotificationFormatting<string>
                    {
                        Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                        Subject = "subject1",
                        ImageSmall = string.Empty,
                        ImageLarge = string.Empty,
                        LinkUrl = "https://app.notifo.io/custom/link",
                        LinkText = "Go to link"
                    }
                });

        var (html, text) = await FormatAsync(jobs);

        await WriteResultFileAsync("template-link.html", html);
        await WriteResultFileAsync("template-link.txt", text);

        foreach (var notification in jobs.Select(x => x.Notification))
        {
            // Test Html result.
            Assert.Contains(notification.Formatting.Body!, html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(notification.Formatting.Subject!, html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(notification.Formatting.LinkUrl!, html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(notification.Formatting.LinkText!, html, StringComparison.OrdinalIgnoreCase);

            // Test Text result.
            Assert.Contains(notification.Formatting.Body!, text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(notification.Formatting.Subject!, text, StringComparison.OrdinalIgnoreCase);
        }

        DoesNotContainPlaceholders(text);
    }

    [Fact]
    public async Task Should_generate_template_with_image()
    {
        var jobs = ToJobs(
            new UserNotification
            {
                UserLanguage = "en",
                Formatting = new NotificationFormatting<string>
                {
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    Subject = "subject1",
                    ImageSmall = "https://raw.githubusercontent.com/notifo-io/notifo/main/backend/src/Notifo/wwwroot/placeholder.png",
                    ImageLarge = "https://raw.githubusercontent.com/notifo-io/notifo/main/backend/src/Notifo/wwwroot/placeholder-large.png"
                }
            });

        var (html, text) = await FormatAsync(jobs);

        await WriteResultFileAsync("template-image.html", html);
        await WriteResultFileAsync("template-image.txt", text);

        foreach (var notification in jobs.Select(x => x.Notification))
        {
            // Test Html result.
            Assert.Contains(notification.Formatting.ImageSmall!, html, StringComparison.OrdinalIgnoreCase);

            // Test Text result.
            Assert.DoesNotContain(notification.Formatting.ImageSmall!, text, StringComparison.OrdinalIgnoreCase);
        }

        DoesNotContainPlaceholders(text);
    }

    [Fact]
    public async Task Should_generate_template_with_tracking_url()
    {
        var jobs = ToJobs(
            new UserNotification
            {
                UserLanguage = "en",
                Formatting = new NotificationFormatting<string>
                {
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    Subject = "subject1",
                    ImageSmall = string.Empty,
                    ImageLarge = string.Empty,
                    ConfirmText = "Got It!"
                },
                TrackSeenUrl = "https://track.notifo.com"
            });

        var (html, text) = await FormatAsync(jobs);

        await WriteResultFileAsync("template-tracking.html", html);
        await WriteResultFileAsync("template-tracking.txt", text);

        foreach (var notification in jobs.Select(x => x.Notification))
        {
            // Test Html result.
            Assert.Contains(notification.TrackSeenUrl!, html, StringComparison.OrdinalIgnoreCase);

            // Test Text result.
            Assert.DoesNotContain(notification.TrackSeenUrl!, text, StringComparison.OrdinalIgnoreCase);
        }

        DoesNotContainPlaceholders(text);
    }

    [Fact]
    public async Task Should_generate_template_with_button()
    {
        var jobs = ToJobs(
            new UserNotification
            {
                UserLanguage = "en",
                Formatting = new NotificationFormatting<string>
                {
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    Subject = "subject1",
                    ImageSmall = string.Empty,
                    ImageLarge = string.Empty,
                    ConfirmText = "Got It!",
                    ConfirmMode = ConfirmMode.Explicit
                },
                ConfirmUrl = "https://confirm.notifo.com"
            });

        var (html, text) = await FormatAsync(jobs);

        await WriteResultFileAsync("template-button.html", html);
        await WriteResultFileAsync("template-button.txt", text);

        foreach (var notification in jobs.Select(x => x.Notification))
        {
            // Test Html result.
            Assert.Contains(notification.Formatting.ConfirmText!, html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(notification.ConfirmUrl!, html, StringComparison.OrdinalIgnoreCase);

            // Test Text result.
            Assert.Contains(notification.ConfirmUrl!, text, StringComparison.OrdinalIgnoreCase);
        }

        DoesNotContainPlaceholders(text);
    }

    [Fact]
    public async Task Should_generate_template_with_button_and_image()
    {
        var jobs = ToJobs(
            new UserNotification
            {
                UserLanguage = "en",
                Formatting = new NotificationFormatting<string>
                {
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    Subject = "subject1",
                    ImageSmall = "https://notifo.io/placeholder.png",
                    ImageLarge = "https://notifo.io/placeholder-large.png",
                    ConfirmText = "Got It!",
                    ConfirmMode = ConfirmMode.Explicit
                },
                ConfirmUrl = "https://confirm.notifo.com"
            });

        var (html, text) = await FormatAsync(jobs);

        await WriteResultFileAsync("template-button-image.html", html);
        await WriteResultFileAsync("template-button-image.txt", text);

        foreach (var notification in jobs.Select(x => x.Notification))
        {
            // Test Html result.
            Assert.Contains(notification.Formatting.ImageSmall!, html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(notification.Formatting.ConfirmText!, html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(notification.ConfirmUrl!, html, StringComparison.OrdinalIgnoreCase);

            // Test Text result.
            Assert.Contains(notification.ConfirmUrl!, text, StringComparison.OrdinalIgnoreCase);
        }

        DoesNotContainPlaceholders(text);
    }

    [Fact]
    public async Task Should_generate_template_with_custom_property()
    {
        var jobs = ToJobs(
            new UserNotification
            {
                UserLanguage = "en",
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "subject1",
                },
                Properties = new NotificationProperties
                {
                    ["myProperty1"] = "Hello World"
                }
            });

        var (html, text) = await FormatAsync(jobs, templateCustom);

        await WriteResultFileAsync("template-properties.html", html);
        await WriteResultFileAsync("template-properties.txt", text);

        foreach (var notification in jobs.Select(x => x.Notification))
        {
            var customValue = notification.Properties!["myProperty1"];

            // Test Html result.
            Assert.Contains(customValue, html, StringComparison.OrdinalIgnoreCase);

            // Test Text result.
            Assert.Contains(customValue, text, StringComparison.OrdinalIgnoreCase);
        }

        DoesNotContainPlaceholders(text);
    }

    [Fact]
    public async Task Should_generate_multi_template()
    {
        var jobs = ToJobs(
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    Subject = "subject1"
                },
                UserLanguage = "en"
            },
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                    Subject = "subject2"
                },
                UserLanguage = "en"
            });

        var (html, text) = await FormatAsync(jobs);

        await WriteResultFileAsync("template-multi.html", html);
        await WriteResultFileAsync("template-multi.txt", text);

        foreach (var notification in jobs.Select(x => x.Notification))
        {
            // Test Html result.
            Assert.Contains(notification.Formatting.Body!, html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(notification.Formatting.Subject, html, StringComparison.OrdinalIgnoreCase);

            // Test Text result.
            Assert.Contains(notification.Formatting.Body!, text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(notification.Formatting.Subject, text, StringComparison.OrdinalIgnoreCase);
        }

        DoesNotContainPlaceholders(text);
    }

    private static void DoesNotContainPlaceholders(string? text)
    {
        Assert.DoesNotContain("<!--", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("-->", text, StringComparison.OrdinalIgnoreCase);
    }

    private async Task WriteResultFileAsync(string path, string? contents)
    {
        var directory = Directory.CreateDirectory($"_out\\{Name}");

        await File.WriteAllTextAsync(Path.Combine(directory.FullName, path), contents);
    }

    private async Task<(string?, string?)> FormatAsync(List<EmailJob> jobs, EmailTemplate? template = null)
    {
        var (message, _) = await emailFormatter.FormatAsync(
            template ?? templateDefault,
            jobs,
            PreviewData.App,
            PreviewData.User);

        return (message?.BodyHtml, message?.BodyText);
    }

    private async Task<EmailTemplate> ParseCustomAsync()
    {
        var source = new EmailTemplate
        {
            BodyHtml = await File.ReadAllTextAsync("Channels/Email/CustomHtml.liquid.mjml"),
            BodyText = await File.ReadAllTextAsync("Channels/Email/CustomText.liquid.text"),
            Subject = await File.ReadAllTextAsync("Channels/Email/CustomSubject.text")
        };

        return await emailFormatter.ParseAsync(source, true);
    }

    private static List<EmailJob> ToJobs(params UserNotification[] notifications)
    {
        var context = new ChannelContext
        {
            App = null!,
            AppId = null!,
            Configuration = [],
            ConfigurationId = Guid.NewGuid(),
            IsUpdate = false,
            Setting = new ChannelSetting(),
            User = null!,
            UserId = null!,
        };

        return notifications.Select(x => new EmailJob(x, context, PreviewData.User.EmailAddress!)).ToList();
    }
}
