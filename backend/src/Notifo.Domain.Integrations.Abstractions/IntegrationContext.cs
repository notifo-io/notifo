// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Domain.Integrations;

public sealed class IntegrationContext
{
    required public IIntegrationAdapter IntegrationAdapter { get; init; }

    required public UpdateStatus UpdateStatusAsync { get; init; }

    required public string IntegrationId { get; init; }

    required public string AppId { get; init; }

    required public string AppName { get; init; }

    required public string CallbackUrl { get; init; }

    required public string CallbackToken { get; init; }

    required public string WebhookUrl { get; init; }

    required public Dictionary<string, string> Properties { get; init; }
}

public delegate Task UpdateStatus(string trackingToken, DeliveryResult result);
