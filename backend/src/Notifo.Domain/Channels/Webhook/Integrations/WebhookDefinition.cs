// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Webhook.Integrations;

public sealed class WebhookDefinition
{
    public string? Name { get; init; }

    public string HttpUrl { get; init; }

    public string HttpMethod { get; init; }

    public bool SendAlways { get; init; }

    public bool SendConfirm { get; init; }
}
