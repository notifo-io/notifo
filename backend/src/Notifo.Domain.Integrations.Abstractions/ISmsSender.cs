// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface ISmsSender : IIntegrationService
{
    Task<DeliveryResult> SendAsync(SmsMessage message,
        CancellationToken ct);
}
