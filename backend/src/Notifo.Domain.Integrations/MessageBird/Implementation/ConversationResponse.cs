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
    public sealed class ConversationResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; }

        [JsonPropertyName("channelId")]
        public string? ChannelId { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("direction")]
        public string? Direction { get; set; }

        [JsonPropertyName("error")]
        public MessageBirdError? Error { get; set; }

        [JsonPropertyName("errors")]
        public MessageBirdError[]? Errors { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ConversationStatus Status { get; set; }
    }

    public enum ConversationStatus
    {
        Accepted,
        Failed,
        Rejected
    }
}
