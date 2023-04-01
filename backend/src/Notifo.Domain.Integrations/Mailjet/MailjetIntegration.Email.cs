// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations.Mailjet;

public sealed partial class MailjetIntegration : IEmailSender
{
    private const int Attempts = 5;

    public async Task<DeliveryResult> SendAsync(IntegrationContext context, EmailMessage request,
        CancellationToken ct)
    {
        var apiKey = ApiKeyProperty.GetString(context.Properties);
        var apiSecret = ApiSecretProperty.GetString(context.Properties);
        var fromEmail = FromEmailProperty.GetString(context.Properties);
        var fromName = FromNameProperty.GetString(context.Properties);

        if (string.IsNullOrWhiteSpace(request.FromEmail))
        {
            request = request with { FromEmail = fromEmail };
        }

        if (string.IsNullOrWhiteSpace(request.FromName))
        {
            request = request with { FromName = fromName };
        }

        // Try a few attempts to get a non-disposed server instance.
        for (var i = 1; i <= Attempts; i++)
        {
            try
            {
                var server = serverPool.GetServer(apiKey, apiSecret);

                await server.SendAsync(request);
                break;
            }
            catch (ObjectDisposedException)
            {
                if (i == Attempts)
                {
                    throw;
                }
            }
        }

        return DeliveryResult.Handled;
    }
}
