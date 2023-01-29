// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface IEmailSender : IIntegration
{
    Task<DeliveryResult> SendAsync(IntegrationContext context, EmailMessage message,
        CancellationToken ct);
}
