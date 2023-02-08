// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public sealed class MessagingMessage : BaseMessage
{
    public string Text { get; set; }

    public IReadOnlyDictionary<string, string> Targets { get; set; }
}
