// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public sealed class IntegrationContext
{
    required public IIntegrationAdapter Adapter { get; init; }

    required public string IntegrationId { get; init; }

    required public string AppId { get; init; }

    required public string AppName { get; init; }

    required public string CallbackUrl { get; init; }

    required public string CallbackToken { get; init; }

    required public string WebhookUrl { get; init; }

    required public Dictionary<string, string> Properties { get; init; }
}
