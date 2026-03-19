// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.Seven;

public sealed partial class SevenSmsIntegration : ISmsSender
{
    public async Task<DeliveryResult> SendAsync(IntegrationContext context, SmsMessage message,
        CancellationToken ct)
    {
        var to = message.To;

        try
        {
            var from = FromProperty.GetString(context.Properties);
            var client = GetClient(context);

            var response = await client.SendSmsAsync(to, message.Text, from, ct);

            var statusCode = response.StatusCode;

            // 100 = SMS sent, 101 = SMS sent to multiple recipients
            var isSuccess = (statusCode == "100" || statusCode == "101")
                && response.Messages?.Any(m => !m.Success) != true;

            if (!isSuccess)
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Seven_ErrorUnknown, to);

                throw new DomainException(errorMessage);
            }

            return DeliveryResult.Sent;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Seven_Error, to, ex.Message);

            throw new DomainException(errorMessage);
        }
    }

    private Implementation.ISevenSmsClient GetClient(IntegrationContext context)
    {
        var apiKey = ApiKeyProperty.GetString(context.Properties);

        return clientPool.GetClient(apiKey);
    }
}
