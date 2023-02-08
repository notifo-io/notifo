// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;

namespace Notifo.Domain.Integrations;

public abstract class EmailSenderTestBase
{
    protected abstract ResolvedIntegration<IEmailSender> CreateSender();

    public static string Address { get; } = TestHelpers.Configuration.GetValue<string>("email:address")!;

    [Fact]
    public async Task Should_send_text_message()
    {
        var (_, context, sender) = CreateSender();

        await sender.SendAsync(context, new EmailMessage
        {
            BodyHtml = null,
            BodyText = "Notifo Test  Body",
            Subject = "Notifo Test Subject",
            ToEmail = Address,
            ToName = Address,
        }, default);
    }

    [Fact]
    public async Task Should_send_html_message()
    {
        var (_, context, sender) = CreateSender();

        await sender.SendAsync(context, new EmailMessage
        {
            BodyHtml = "<div style=\"color: red\">Notifo Test HTML Body</div>",
            BodyText = null,
            Subject = "Notifo Test Subject",
            ToEmail = Address,
            ToName = Address,
        }, default);
    }

    [Fact]
    public async Task Should_send_multipart_message()
    {
        var (_, context, sender) = CreateSender();

        await sender.SendAsync(context, new EmailMessage
        {
            BodyHtml = "<div style=\"color: red\">Notifo Test HTML Body</div>",
            BodyText = "Notifo Test TEXT Body",
            Subject = "Notifo Test Subject",
            ToEmail = Address,
            ToName = Address,
        }, default);
    }

    protected static IntegrationContext BuildContext(Dictionary<string, string> properties)
    {
        return new IntegrationContext
        {
            UpdateStatusAsync = null!,
            AppId = string.Empty,
            AppName = string.Empty,
            CallbackToken = string.Empty,
            CallbackUrl = string.Empty,
            IntegrationAdapter = null!,
            IntegrationId = string.Empty,
            Properties = properties,
            WebhookUrl = string.Empty,
        };
    }
}
