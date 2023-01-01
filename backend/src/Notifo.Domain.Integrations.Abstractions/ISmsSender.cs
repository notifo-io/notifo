// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface ISmsSender : IIntegrationService
{
    Task<SmsResult> SendAsync(SmsMessage message,
        CancellationToken ct);
}
