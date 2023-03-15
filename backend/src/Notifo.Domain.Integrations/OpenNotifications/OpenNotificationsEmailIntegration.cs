// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenNotifications;

namespace Notifo.Domain.Integrations.OpenNotifications;

public sealed class OpenNotificationsEmailIntegration : OpenNotificationsIntegrationBase, IEmailSender
{
    public OpenNotificationsEmailIntegration(string fullName, string providerName, ProviderInfoDto providerInfo, IOpenNotificationsClient client)
        : base(fullName, providerName, providerInfo, client)
    {
    }

    public async Task<DeliveryResult> SendAsync(IntegrationContext context, EmailMessage request,
        CancellationToken ct)
    {
        try
        {
            var requestDto = new SendEmailRequestDto
            {
                Context = context.ToContext(),
                Payload = new EmailPayloadDto
                {
                    BodyHtml = request.BodyHtml,
                    BodyText = request.BodyText,
                    FromEmail = request.FromEmail,
                    FromName = request.FromName!,
                    Subject = request.Subject,
                    To = request.ToEmail
                },
                Properties = context.Properties.ToProperties(Definition),
                Provider = ProviderName,
                TrackingToken = "none",
                TrackingWebhookUrl = "none",
            };

            var status = await Client.Providers.SendEmailAsync(requestDto, ct);

            return status.ToDeliveryResult();
        }
        catch (OpenNotificationsException ex)
        {
            throw ex.ToDomainException();
        }
    }
}
