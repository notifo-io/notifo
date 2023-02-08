// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public interface IIntegrationUrl
{
    string WebhookUrl(string appId, string integrationId);

    string CallbackUrl();
}
