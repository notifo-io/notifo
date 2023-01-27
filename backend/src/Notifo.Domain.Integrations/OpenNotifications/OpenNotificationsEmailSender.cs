// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenNotifications;

namespace Notifo.Domain.Integrations.OpenNotifications;

public sealed class OpenNotificationsEmailSender : IEmailSender
{
    private readonly IntegrationDefinition definition;
    private readonly IntegrationConfiguration configured;
    private readonly IOpenNotificationsClient client;

    public string Name => definition.Type;

    public OpenNotificationsEmailSender(
        IntegrationDefinition definition,
        IntegrationConfiguration configured,
        IOpenNotificationsClient client)
    {
        this.definition = definition;
        this.configured = configured;
        this.client = client;
    }

    public async Task SendAsync(EmailMessage request,
        CancellationToken ct = default)
    {
        var requestDto = new SendEmailRequestDto
        {
            Properties = new Dictionary<string, object?>(),
            Payload = new EmailPayloadDto
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

        await client.Providers.SendEmailAsync(requestDto, ct);
    }
}
