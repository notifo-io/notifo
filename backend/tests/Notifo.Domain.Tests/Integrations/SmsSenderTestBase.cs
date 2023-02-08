// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;

namespace Notifo.Domain.Integrations;

public abstract class SmsSenderTestBase
{
    protected abstract ResolvedIntegration<ISmsSender> CreateSender();

    public static string PhoneNumber { get; } = TestHelpers.Configuration.GetValue<string>("sms:phoneNumber")!;

    [Fact]
    public async Task Should_send_text_message()
    {
        var (_, context, sender) = CreateSender();

        await sender.SendAsync(context, new SmsMessage
        {
            To = PhoneNumber,
            Text = "Notifo sample SMS",
        }, default);
    }

    [Fact]
    public async Task Should_send_long_text_message()
    {
        var (_, context, sender) = CreateSender();

        await sender.SendAsync(context, new SmsMessage
        {
            To = PhoneNumber,
            Text = "Notifo sample SMS with very long text that is over 140 characters to test longer SMS messages. The limit is not the biggest deal anymore, so it should just sned fine..",
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
