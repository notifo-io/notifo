// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.MessageBird;

public sealed class IntegratedMessageBirdIntegration : IIntegration, ISmsSender, IIntegrationHook
{
    private readonly MessageBirdOptions smsOptions;
    private readonly MessageBirdSmsIntegration smsSender;

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "MessageBirdIntegrated",
            Texts.MessageBirdIntegrated_Name,
            "./integrations/messagebird.svg",
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
        // context.Properties[MessageBirdSmsIntegration.PhoneNumbersProperty.Name] = smsOptions.PhoneNumbers;
    }
}
