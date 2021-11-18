// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Notifo.Domain.Channels.Email
{
    [Trait("Category", "Dependencies")]
    public class EmailTemplateTests : IClassFixture<EmailTemplateFixture>
    {
        public EmailTemplateFixture _ { get; }

        public EmailTemplateTests(EmailTemplateFixture fixture)
        {
            _ = fixture;
        }

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

            var email = await _.EmailFormatter.FormatAsync(jobs,  _.EmailTemplate, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

            foreach (var notification in jobs.Select(x => x.Notification))
            {
                Assert.Contains(notification.Formatting.Body, html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.Formatting.Subject, html, StringComparison.OrdinalIgnoreCase);

                Assert.Contains(notification.Formatting.Body, email.BodyText, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(notification.Formatting.Subject, text, StringComparison.OrdinalIgnoreCase);
            }

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

            var email = await _.EmailFormatter.FormatAsync(jobs, _.EmailTemplate, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

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

            var email = await _.EmailFormatter.FormatAsync(jobs, _.EmailTemplate, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

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

            var email = await _.EmailFormatter.FormatAsync(jobs, _.EmailTemplate, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

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

            var email = await _.EmailFormatter.FormatAsync(jobs, _.EmailTemplate, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

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

            var email = await _.EmailFormatter.FormatAsync(jobs, _.EmailTemplate, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

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

            var email = await _.EmailFormatter.FormatAsync(jobs, _.EmailTemplate, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

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

        private static List<EmailJob> ToJobs(params UserNotification[] notifications)
        {
            return notifications.Select(x => new EmailJob(x, new NotificationSetting(), "john.doe@email.com")).ToList();
        }
    }
}
