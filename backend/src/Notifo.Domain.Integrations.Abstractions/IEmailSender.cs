// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface IEmailSender : IIntegrationService
{
    Task SendAsync(EmailMessage request,
        CancellationToken ct = default);
}
