// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http.Json;

namespace Notifo.Domain.Integrations.Http;

public sealed partial class HttpIntegration : IWebhookSender
{
    public async Task<DeliveryResult> SendAsync(IntegrationContext context, WebhookMessage message,
        CancellationToken ct)
    {
        var sendAlways = SendAlwaysProperty.GetBoolean(context.Properties);
        var sendConfirm = SendConfirmProperty.GetBoolean(context.Properties);

        var send = sendAlways || (!message.IsUpdate || sendConfirm);

        if (!send)
        {
            return DeliveryResult.Skipped();
        }

        var httpClient = httpClientFactory.CreateClient("Unsafe");

        var httpUrl = HttpUrlProperty.GetString(context.Properties);
        var httpMethod = HttpMethodProperty.GetString(context.Properties);
        var httpRequest = new HttpRequestMessage(new HttpMethod(httpMethod), httpUrl)
        {
            Content = JsonContent.Create(message.Payload)
        };

        using var response = await httpClient.SendAsync(httpRequest, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = $"Webhook failed with status code {(int)response.StatusCode}.";

            // Throw an exception for temporary errors, so that the webhook is retried.
            if (response.StatusCode.IsTransient())
            {
                throw new HttpIntegrationException(error, (int)response.StatusCode);
            }

            return DeliveryResult.Failed(error);
        }

        return DeliveryResult.Handled;
    }
}
