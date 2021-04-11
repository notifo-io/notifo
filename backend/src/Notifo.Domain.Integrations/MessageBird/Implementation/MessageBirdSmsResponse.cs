// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;

namespace Notifo.Domain.Integrations.MessageBird.Implementation
{
    public sealed class MessageBirdSmsResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("recipients")]
        public MessageBirdSmsResponseRecipients Recipients { get; set; }
    }

    public sealed class MessageBirdSmsResponseRecipients
    {
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("totalSentCount")]
        public int TotalSentCount { get; set; }

        [JsonPropertyName("totalDeliveredCount")]
        public int TotalDeliveredCount { get; set; }

        [JsonPropertyName("totalDeliveryFailedCount")]
        public int TotalDeliveryFailedCount { get; set; }

        [JsonPropertyName("items")]
        public MessageBirdSmsResponseRecipient[] Items { get; set; }
    }

    public sealed class MessageBirdSmsResponseRecipient
    {
        [JsonPropertyName("recipient")]
        public long Recipient { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MessageBirdStatus Status { get; set; }
    }
}