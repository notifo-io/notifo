// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface ISmsSender : IIntegration
{
    Task<DeliveryResult> SendAsync(IntegrationContext context, SmsMessage message,
        CancellationToken ct);
}
