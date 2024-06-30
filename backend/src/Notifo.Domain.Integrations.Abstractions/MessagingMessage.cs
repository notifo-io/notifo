// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public sealed class MessagingMessage : BaseMessage
{
    public IReadOnlyDictionary<string, string> Targets { get; set; }

    public string Text { get; set; }

    public string? Body { get; init; }

    public string? ImageLarge { get; init; }

    public string? ImageSmall { get; init; }

    public string? LinkText { get; init; }

    public string? LinkUrl { get; init; }
}
