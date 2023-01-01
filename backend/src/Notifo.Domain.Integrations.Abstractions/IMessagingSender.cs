// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface IMessagingSender : IIntegrationService
{
    void AddTargets(MessagingTargets job, UserContext user);

    Task<MessagingResult> SendAsync(MessagingMessage request,
        CancellationToken ct);
}
