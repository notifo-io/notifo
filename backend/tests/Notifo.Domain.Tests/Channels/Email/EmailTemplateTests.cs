// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
            var notifications = new List<UserNotification>
            {
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
                }
            };

            var email = await _.EmailFormatter.FormatAsync(notifications, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

            foreach (var notification in notifications)
            {
                Assert.Contains(notification.Formatting.Body, html);
                Assert.Contains(notification.Formatting.Subject, html);

                Assert.Contains(notification.Formatting.Body, email.BodyText);
                Assert.Contains(notification.Formatting.Subject, text);
            }

            await File.WriteAllTextAsync("_out\\template-single.html", html);
            await File.WriteAllTextAsync("_out\\template-single.txt", text);
        }

        [Fact]
        public async Task Should_generate_template_with_link()
        {
            var notifications = new List<UserNotification>
            {
                new UserNotification
                {
                    UserLanguage = "en",
                    Formatting = new NotificationFormatting<string>
                    {
                        Body = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr",
                        Subject = "subject1",
                        ImageSmall = string.Empty,
                        ImageLarge = string.Empty,
                        LinkUrl = "https://app.notifo.io",
                        LinkText = "Go to link"
                    }
                }
            };

            var email = await _.EmailFormatter.FormatAsync(notifications, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

            foreach (var notification in notifications)
            {
                Assert.Contains(notification.Formatting.Body, html);
                Assert.Contains(notification.Formatting.Subject, html);
                Assert.Contains(notification.Formatting.LinkUrl, html);

                Assert.Contains(notification.Formatting.Body, text);
                Assert.Contains(notification.Formatting.Subject, text);
            }

            DoesNotContainPlaceholders(text);

            await File.WriteAllTextAsync("_out\\template-link.html", html);
            await File.WriteAllTextAsync("_out\\template-link.txt", text);
        }

        [Fact]
        public async Task Should_generate_template_with_image()
        {
            var notifications = new List<UserNotification>
            {
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
                }
            };

            var email = await _.EmailFormatter.FormatAsync(notifications, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

            foreach (var notification in notifications)
            {
                Assert.Contains(notification.Formatting.ImageSmall, html);

                Assert.DoesNotContain(notification.Formatting.ImageSmall, text);
            }

            DoesNotContainPlaceholders(text);

            await File.WriteAllTextAsync("_out\\template-image.html", html);
            await File.WriteAllTextAsync("_out\\template-image.txt", text);
        }

        [Fact]
        public async Task Should_generate_template_with_tracking_url()
        {
            var notifications = new List<UserNotification>
            {
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
                    TrackingUrl = "https://track.notifo.com"
                }
            };

            var email = await _.EmailFormatter.FormatAsync(notifications, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

            foreach (var notification in notifications)
            {
                Assert.Contains(notification.TrackingUrl, html);

                Assert.DoesNotContain(notification.TrackingUrl, text);
            }

            DoesNotContainPlaceholders(text);

            await File.WriteAllTextAsync("_out\\template-tracking.html", html);
            await File.WriteAllTextAsync("_out\\template-tracking.txt", text);
        }

        [Fact]
        public async Task Should_generate_template_with_button()
        {
            var notifications = new List<UserNotification>
            {
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
                }
            };

            var email = await _.EmailFormatter.FormatAsync(notifications, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

            foreach (var notification in notifications)
            {
                Assert.Contains(notification.Formatting.ConfirmText, html);
                Assert.Contains(notification.ConfirmUrl, html);

                Assert.Contains(notification.ConfirmUrl, text);
            }

            DoesNotContainPlaceholders(text);

            await File.WriteAllTextAsync("_out\\template-button.html", html);
            await File.WriteAllTextAsync("_out\\template-button.txt", text);
        }

        [Fact]
        public async Task Should_generate_template_with_button_and_image()
        {
            var notifications = new List<UserNotification>
            {
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
                }
            };

            var email = await _.EmailFormatter.FormatAsync(notifications, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

            foreach (var notification in notifications)
            {
                Assert.Contains(notification.Formatting.ImageSmall, html);
                Assert.Contains(notification.Formatting.ConfirmText, html);
                Assert.Contains(notification.ConfirmUrl, html);

                Assert.Contains(notification.ConfirmUrl, text);
            }

            DoesNotContainPlaceholders(text);

            await File.WriteAllTextAsync("_out\\template-button-image.html", html);
            await File.WriteAllTextAsync("_out\\template-button-image.txt", text);
        }

        [Fact]
        public async Task Should_generate_multi_template()
        {
            var notifications = new List<UserNotification>
            {
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
                }
            };

            var email = await _.EmailFormatter.FormatAsync(notifications, _.App, new User());

            var html = email.BodyHtml;
            var text = email.BodyText;

            foreach (var notification in notifications)
            {
                Assert.Contains(notification.Formatting.Body, html);
                Assert.Contains(notification.Formatting.Subject, html);

                Assert.Contains(notification.Formatting.Body, text);
                Assert.Contains(notification.Formatting.Subject, text);
            }

            DoesNotContainPlaceholders(text);

            await File.WriteAllTextAsync("_out\\template-multi.html", html);
            await File.WriteAllTextAsync("_out\\template-multi.txt", text);
        }

        private static void DoesNotContainPlaceholders(string? text)
        {
            Assert.DoesNotContain("<!--", text);
            Assert.DoesNotContain("-->", text);
        }
    }
}
