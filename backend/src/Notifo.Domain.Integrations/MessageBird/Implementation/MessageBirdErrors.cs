// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Notifo.Domain.Integrations.MessageBird.Implementation;

public sealed class MessageBirdErrors
{
    [JsonPropertyName("errors")]
    public MessageBirdError[] Errors { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
