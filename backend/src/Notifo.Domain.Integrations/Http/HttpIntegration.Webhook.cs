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

        var httpUrl = HttpUrlProperty.GetString(context.Properties);
        var httpMethod = HttpMethodProperty.GetString(context.Properties);

        var client = httpClientFactory.CreateClient();

        var request = new HttpRequestMessage(new HttpMethod(httpMethod), httpUrl)
        {
            Content = JsonContent.Create(message.Payload)
        };

        await client.SendAsync(request, ct);

        return DeliveryResult.Handled;
    }
}
