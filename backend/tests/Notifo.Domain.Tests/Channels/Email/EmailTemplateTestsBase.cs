﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Notifo.Domain.Apps;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Xunit;

#pragma warning disable MA0056 // Do not call overridable members in constructor

namespace Notifo.Domain.Channels.Email
{
    public abstract class EmailTemplateTestsBase
    {
        private readonly App app = new App("1", default);
        private readonly EmailTemplate emailTemplate;
        private readonly IEmailFormatter emailFormatter;

        protected EmailTemplateTestsBase()
        {
            var emailUrl = A.Fake<IEmailUrl>();

            A.CallTo(() => emailUrl.EmailPreferences(A<string>._, A<string>._))
                .Returns("url/to/email-preferences");

            var imageFormatter = A.Fake<IImageFormatter>();

            A.CallTo(() => imageFormatter.Format(A<string>._, A<string>._))
                .ReturnsLazily(x => x.GetArgument<string>(0) ?? string.Empty);

            emailFormatter = CreateFormatter(emailUrl, imageFormatter);
            emailTemplate = emailFormatter.CreateInitialAsync().AsTask().Result;
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

            foreach (var notification in jobs.Select(x => x.Notification))
            {
                Assert.Contains(notification.Formatting.Body, html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.Formatting.Subject, html, StringComparison.OrdinalIgnoreCase);

                Assert.Contains(notification.Formatting.Body, text, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.Formatting.Subject, text, StringComparison.OrdinalIgnoreCase);
            }

            Assert.Contains("url/to/email-preferences", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("url/to/email-preferences", text, StringComparison.OrdinalIgnoreCase);

            await File.WriteAllTextAsync("_out\\template-single.html", html);
            await File.WriteAllTextAsync("_out\\template-single.txt", text);
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

            foreach (var notification in jobs.Select(x => x.Notification))
            {
                Assert.Contains(notification.Formatting.Body, html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.Formatting.Subject, html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.Formatting.LinkUrl, html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.Formatting.LinkText, html, StringComparison.OrdinalIgnoreCase);

                Assert.Contains(notification.Formatting.Body, text, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.Formatting.Subject, text, StringComparison.OrdinalIgnoreCase);
            }

            DoesNotContainPlaceholders(text);

            await File.WriteAllTextAsync("_out\\template-link.html", html);
            await File.WriteAllTextAsync("_out\\template-link.txt", text);
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
                        ImageSmall = "https://via.placeholder.com/100",
                        ImageLarge = "https://via.placeholder.com/600"
                    }
                });

            var (html, text) = await FormatAsync(jobs);

            foreach (var notification in jobs.Select(x => x.Notification))
            {
                Assert.Contains(notification.Formatting.ImageSmall, html, StringComparison.OrdinalIgnoreCase);

                Assert.DoesNotContain(notification.Formatting.ImageSmall, text, StringComparison.OrdinalIgnoreCase);
            }

            DoesNotContainPlaceholders(text);

            await File.WriteAllTextAsync("_out\\template-image.html", html);
            await File.WriteAllTextAsync("_out\\template-image.txt", text);
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

            foreach (var notification in jobs.Select(x => x.Notification))
            {
                Assert.Contains(notification.TrackSeenUrl, html, StringComparison.OrdinalIgnoreCase);

                Assert.DoesNotContain(notification.TrackSeenUrl, text, StringComparison.OrdinalIgnoreCase);
            }

            DoesNotContainPlaceholders(text);

            await File.WriteAllTextAsync("_out\\template-tracking.html", html);
            await File.WriteAllTextAsync("_out\\template-tracking.txt", text);
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

            foreach (var notification in jobs.Select(x => x.Notification))
            {
                Assert.Contains(notification.Formatting.ConfirmText, html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.ConfirmUrl, html, StringComparison.OrdinalIgnoreCase);

                Assert.Contains(notification.ConfirmUrl, text, StringComparison.OrdinalIgnoreCase);
            }

            DoesNotContainPlaceholders(text);

            await File.WriteAllTextAsync("_out\\template-button.html", html);
            await File.WriteAllTextAsync("_out\\template-button.txt", text);
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
                        ImageSmall = "https://via.placeholder.com/100",
                        ImageLarge = "https://via.placeholder.com/600",
                        ConfirmText = "Got It!",
                        ConfirmMode = ConfirmMode.Explicit
                    },
                    ConfirmUrl = "https://confirm.notifo.com"
                });

            var (html, text) = await FormatAsync(jobs);

            foreach (var notification in jobs.Select(x => x.Notification))
            {
                Assert.Contains(notification.Formatting.ImageSmall, html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.Formatting.ConfirmText, html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.ConfirmUrl, html, StringComparison.OrdinalIgnoreCase);

                Assert.Contains(notification.ConfirmUrl, text, StringComparison.OrdinalIgnoreCase);
            }

            DoesNotContainPlaceholders(text);

            await File.WriteAllTextAsync("_out\\template-button-image.html", html);
            await File.WriteAllTextAsync("_out\\template-button-image.txt", text);
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

            foreach (var notification in jobs.Select(x => x.Notification))
            {
                Assert.Contains(notification.Formatting.Body, html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.Formatting.Subject, html, StringComparison.OrdinalIgnoreCase);

                Assert.Contains(notification.Formatting.Body, text, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.Formatting.Subject, text, StringComparison.OrdinalIgnoreCase);
            }

            DoesNotContainPlaceholders(text);

            await File.WriteAllTextAsync("_out\\template-multi.html", html);
            await File.WriteAllTextAsync("_out\\template-multi.txt", text);
        }

        private static void DoesNotContainPlaceholders(string? text)
        {
            Assert.DoesNotContain("<!--", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("-->", text, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<(string?, string?)> FormatAsync(List<EmailJob> jobs)
        {
            var formatted = await emailFormatter.FormatAsync(jobs, emailTemplate, app, new User("1", "1", default));

            return (formatted.Message?.BodyHtml, formatted.Message?.BodyText);
        }

        private static List<EmailJob> ToJobs(params UserNotification[] notifications)
        {
            return notifications.Select(x => new EmailJob(x, new ChannelSetting(), "john.doe@email.com")).ToList();
        }
    }
}
