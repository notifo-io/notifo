// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface IMessagingSender : IIntegration
{
    void AddTargets(IDictionary<string, string> targets, UserInfo user);

    Task<DeliveryResult> SendAsync(IntegrationContext context, MessagingMessage message,
        CancellationToken ct);
}
