// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Domain.Integrations.MessageBird.Implementation
{
    public sealed class WhatsAppStatus
    {
        [JsonIgnore]
        public Guid Reference { get; set; }

        [JsonIgnore]
        public string To { get; internal set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("message")]
        public WhatsAppStatusMessage Message { get; set; }

        [JsonPropertyName("error")]
        public MessageBirdError? Error { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public sealed class WhatsAppStatusMessage
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MessageBirdStatus Status { get; set; }
    }
}
