// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface IMobilePushSender : IIntegration
{
    Task<DeliveryResult> SendAsync(IntegrationContext context, MobilePushMessage message,
        CancellationToken ct);
}
