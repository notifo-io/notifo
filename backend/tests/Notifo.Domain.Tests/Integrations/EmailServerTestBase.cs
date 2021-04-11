// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Notifo.Domain.Channels.Email;
using Xunit;

namespace Notifo.Domain.Integrations
{
    public abstract class EmailServerTestBase
    {
        protected abstract IEmailSender CreateServer();

        public static string Address { get; } = TestHelpers.Configuration.GetValue<string>("email:address");

        [Fact]
        public async Task Should_send_text_message()
        {
            var server = CreateServer();

            await server.SendAsync(new EmailMessage
            {
                FromEmail = Address,
                FromName = Address,
                RecipientEmail = Address,
                RecipientName = Address,
                BodyHtml = null,
                BodyText = "Notifo Test TEXT Body",
                Subject = "Notifo Test Subject"
            });
        }

        [Fact]
        public async Task Should_send_html_message()
        {
            var server = CreateServer();

            await server.SendAsync(new EmailMessage
            {
                FromEmail = Address,
                FromName = Address,
                RecipientEmail = Address,
                RecipientName = Address,
                BodyHtml = "<div style=\"color: red\">Notifo Test HTML Body</div>",
                BodyText = null,
                Subject = "Notifo Test Subject"
            });
        }

        [Fact]
        public async Task Should_send_multipart_message()
        {
            var server = CreateServer();

            await server.SendAsync(new EmailMessage
            {
                FromEmail = Address,
                FromName = Address,
                RecipientEmail = Address,
                RecipientName = Address,
                BodyHtml = "<div style=\"color: red\">Notifo Test HTML Body</div>",
                BodyText = "Notifo Test TEXT Body",
                Subject = "Notifo Test Subject"
            });
        }
    }
}
