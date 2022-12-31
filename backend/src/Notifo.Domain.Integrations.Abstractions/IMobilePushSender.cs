// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface IMobilePushSender : IIntegrationService
{
    Task SendAsync(MobilePushMessage message,
        CancellationToken ct);
}
