// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Messaging;

namespace Notifo.Domain.Integrations;

public sealed class IntegrationContext
{
    required public IIntegrationAdapter IntegrationAdapter { get; init; }

    required public IMessagingCallback MessagingCallback { get; init; }

    required public ISmsCallback SmsCallback { get; init; }

    required public string IntegrationId { get; init; }

    required public string AppId { get; init; }

    required public string AppName { get; init; }

    required public string CallbackUrl { get; init; }

    required public string CallbackToken { get; init; }

    required public string WebhookUrl { get; init; }

    required public Dictionary<string, string> Properties { get; init; }
}
