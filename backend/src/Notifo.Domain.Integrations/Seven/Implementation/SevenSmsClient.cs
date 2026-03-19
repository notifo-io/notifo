// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.Seven.Implementation;

public sealed class SevenSmsClient(string apiKey, IHttpClientFactory httpClientFactory) : ISevenSmsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };

    public async Task<SevenSmsResponse> SendSmsAsync(string to, string text, string? from,
        CancellationToken ct)
    {
        using var httpClient = httpClientFactory.CreateClient();

        httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        httpClient.DefaultRequestHeaders.Add("SentWith", "notifo");
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var request = new SevenSmsMessage
        {
            To = to,
            Text = text,
            From = from
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://gateway.seven.io/api/sms", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);

            throw new HttpIntegrationException<object>(
                $"Seven API request failed with status {(int)response.StatusCode}: {body}",
                (int)response.StatusCode);
        }

        var result = await JsonSerializer.DeserializeAsync<SevenSmsResponse>(
            await response.Content.ReadAsStreamAsync(ct), JsonOptions, ct);

        return result ?? throw new HttpIntegrationException<object>("Failed to deserialize Seven API response.");
    }
}
