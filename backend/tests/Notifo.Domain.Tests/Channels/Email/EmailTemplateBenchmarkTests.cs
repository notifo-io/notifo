// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Notifo.Domain.Channels.Email
{
    [Trait("Category", "Dependencies")]
    public class EmailTemplateBenchmarkTests : IClassFixture<EmailTemplateFixture>
    {
        public EmailTemplateFixture _ { get; }

        public EmailTemplateBenchmarkTests(EmailTemplateFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_generate_template_with_button()
        {
            var jobs = ToJobs(new UserNotification
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

            for (var i = 0; i < 10; i++)
            {
                await _.EmailFormatter.FormatAsync(jobs, _.EmailTemplate, _.App, new User());
            }

            var watch = Stopwatch.StartNew();

            for (var i = 0; i < 1000; i++)
            {
                await _.EmailFormatter.FormatAsync(jobs, _.EmailTemplate, _.App, new User());
            }

            watch.Stop();

            var elapsed = (double)watch.ElapsedMilliseconds / 1000;

            Assert.InRange(elapsed, 0, 20);
        }

        private static List<EmailJob> ToJobs(params UserNotification[] notifications)
        {
            return notifications.Select(x => new EmailJob(x, new NotificationSetting(), "john.doe@email.com")).ToList();
        }
    }
}
