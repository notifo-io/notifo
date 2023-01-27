// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface IWebhookSender
{
    Task SendAsync(WebhookMessage message,
        CancellationToken ct = default);
}
