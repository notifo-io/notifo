// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Channels.Email;
using Xunit;

namespace Notifo.Domain.Integrations;

public abstract class EmailSenderTestBase
{
    protected abstract IEmailSender CreateSender();

    public static string Address { get; } = TestHelpers.Configuration.GetValue<string>("email:address")!;

    [Fact]
    public async Task Should_send_text_message()
    {
        var sender = CreateSender();

        await sender.SendAsync(new EmailMessage
        {
            ToEmail = Address,
            ToName = Address,
            BodyHtml = null,
            BodyText = "Notifo Test TEXT Body",
            Subject = "Notifo Test Subject"
        });
    }

    [Fact]
    public async Task Should_send_html_message()
    {
        var sender = CreateSender();

        await sender.SendAsync(new EmailMessage
        {
            ToEmail = Address,
            ToName = Address,
            BodyHtml = "<div style=\"color: red\">Notifo Test HTML Body</div>",
            BodyText = null,
            Subject = "Notifo Test Subject"
        });
    }

    [Fact]
    public async Task Should_send_multipart_message()
    {
        var sender = CreateSender();

        await sender.SendAsync(new EmailMessage
        {
            ToEmail = Address,
            ToName = Address,
            BodyHtml = "<div style=\"color: red\">Notifo Test HTML Body</div>",
            BodyText = "Notifo Test TEXT Body",
            Subject = "Notifo Test Subject"
        });
    }
}
