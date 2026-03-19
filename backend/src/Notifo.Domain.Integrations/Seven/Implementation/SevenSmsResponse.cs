// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Domain.Integrations.Seven.Implementation;

public sealed class SevenSmsResponse
{
    [JsonPropertyName("success")]
    public string StatusCode { get; set; }

    [JsonPropertyName("total_price")]
    public decimal TotalPrice { get; set; }

    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }

    [JsonPropertyName("sms_type")]
    public string SmsType { get; set; }

    [JsonPropertyName("messages")]
    public SevenSmsResponseMessage[] Messages { get; set; }
}

public sealed class SevenSmsResponseMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("sender")]
    public string Sender { get; set; }

    [JsonPropertyName("recipient")]
    public string Recipient { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("encoding")]
    public string Encoding { get; set; }
}
