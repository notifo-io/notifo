// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenNotifications;

namespace Notifo.Domain.Integrations.OpenNotifications;

public sealed class OpenNotificationsSmsSender : ISmsSender
{
    private readonly IntegrationContext context;
    private readonly IntegrationDefinition definition;
    private readonly IntegrationConfiguration configured;
    private readonly IOpenNotificationsClient client;

    public string Name => definition.Type;

    public OpenNotificationsSmsSender(
        IntegrationContext context,
        IntegrationDefinition definition,
        IntegrationConfiguration configured,
        IOpenNotificationsClient client)
    {
        this.context = context;
        this.definition = definition;
        this.configured = configured;
        this.client = client;
    }

    public async Task<DeliveryResult> SendAsync(SmsMessage message,
        CancellationToken ct)
    {
        /*
        var requestDto = new SendEmailRequestDto
        {
            Properties = new Dictionary<string, object?>(),
            Payload = new Sms
            {
                BodyHtml = request.BodyHtml,
                BodyText = request.BodyText,
                FromEmail = request.FromEmail,
                FromName = request.FromName!,
                Subject = request.Subject,
                To = request.ToEmail
            },
            Context = new RequestContextDto
            {
            }
        };

        foreach (var property in definition.Properties)
        {
            object? value;

            if (property.Type == PropertyType.Boolean)
            {
                value = property.GetBoolean(context.Properties);
            }
            else if (property.Type == PropertyType.Number)
            {
                value = property.GetNumber(context.Properties);
            }
            else
            {
                value = property.GetString(context.Properties);
            }

            requestDto.Properties.Add(property.Name, value);
        }

        await client.Providers.SendSmsAsync(requestDto, ct);
        */
        return default;
    }
}
