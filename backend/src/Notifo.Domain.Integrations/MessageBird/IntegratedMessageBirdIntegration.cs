// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.MessageBird;

public sealed class IntegratedMessageBirdIntegration : IIntegration, ISmsSender, IIntegrationHook
{
    private readonly MessageBirdOptions smsOptions;
    private readonly MessageBirdSmsIntegration smsSender;
    private readonly string phoneNumbers;

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "MessageBirdIntegrated",
            Texts.MessageBirdIntegrated_Name,
            "<svg xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' viewBox='0 0 66 55' fill='#fff' fill-rule='evenodd' stroke='#000' stroke-linecap='round' stroke-linejoin='round'><use xlink:href='#A' x='.5' y='.5'/><symbol id='A' overflow='visible'><path d='M57.425 9.57c-2.568 0-4.863 1.284-6.264 3.23l-9.454 13.228a1.9 1.9 0 0 1-1.556.817 1.91 1.91 0 0 1-1.906-1.906c0-.39.117-.778.31-1.05l7.975-11.945c.817-1.206 1.284-2.684 1.284-4.28C47.814 3.424 44.39 0 40.15 0H0v7.664h34.393c0 2.1-1.712 3.852-3.852 3.852H0c0 2.723.584 5.33 1.595 7.664H26.69c0 2.1-1.712 3.852-3.852 3.852H3.852a19.12 19.12 0 0 0 15.368 7.664h9.454a1.91 1.91 0 0 1 1.906 1.906 1.91 1.91 0 0 1-1.906 1.906H19.18L6.34 53.688h23.227c10.62 0 19.647-6.925 22.8-16.496l4.32-13.11c1.4-4.24 3.968-7.976 7.314-10.816-1.323-2.218-3.774-3.696-6.575-3.696zm0 5.252c-.778 0-1.44-.66-1.44-1.44a1.46 1.46 0 0 1 1.44-1.44 1.46 1.46 0 0 1 1.44 1.44c0 .817-.66 1.44-1.44 1.44z' stroke='none' fill='#2481d7' fill-rule='nonzero'/></symbol></svg>",
            new List<IntegrationProperty>(),
            new List<IntegrationProperty>(),
            new HashSet<string>
            {
                Providers.Sms
            })
        {
            Description = Texts.MessageBirdIntegrated_Description
        };

    public IntegratedMessageBirdIntegration(IOptions<MessageBirdOptions> smsOptions, MessageBirdSmsIntegration smsSender)
    {
        this.smsOptions = smsOptions.Value;
        this.smsSender = smsSender;

        if (smsOptions.Value.PhoneNumbers?.Count  > 0)
        {
            var sb = new StringBuilder();

            foreach (var (key, value) in smsOptions.Value.PhoneNumbers)
            {
                sb.Append(key);
                sb.Append(':');
                sb.Append(value);
                sb.AppendLine();
            }

            phoneNumbers = sb.ToString();
        }
    }

    public Task<DeliveryResult> SendAsync(IntegrationContext context, SmsMessage message,
        CancellationToken ct)
    {
        FillContext(context);

        return smsSender.SendAsync(context, message, ct);
    }

    public Task HandleRequestAsync(IntegrationContext context, HttpContext httpContext,
        CancellationToken ct)
    {
        FillContext(context);

        return smsSender.HandleRequestAsync(context, httpContext, ct);
    }

    private void FillContext(IntegrationContext context)
    {
        context.Properties[MessageBirdSmsIntegration.AccessKeyProperty.Name] = smsOptions.AccessKey;
        context.Properties[MessageBirdSmsIntegration.PhoneNumberProperty.Name] = smsOptions.PhoneNumber;
        context.Properties[MessageBirdSmsIntegration.PhoneNumbersProperty.Name] = phoneNumbers;
    }
}
