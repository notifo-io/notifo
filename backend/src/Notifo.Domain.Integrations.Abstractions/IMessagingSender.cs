// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface IMessagingSender : IIntegrationService
{
    void AddTargets(IDictionary<string, string> targets, UserContext user);

    Task<DeliveryResult> SendAsync(MessagingMessage request, IReadOnlyDictionary<string, string> targets,
        CancellationToken ct);
}
