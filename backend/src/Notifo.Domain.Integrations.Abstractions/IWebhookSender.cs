// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface IWebhookSender
{
    Task<DeliveryResult> SendAsync(IntegrationContext context, WebhookMessage message,
        CancellationToken ct);
}
