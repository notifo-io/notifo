// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http.Json;

#pragma warning disable MA0048 // File name must match type name

namespace TestSuite.ApiTests;

internal sealed class ReceivedEmail
{
    public int Id { get; set; }

    public string Sender { get; set; }

    public string Subject { get; set; }

    public string[] Recipients { get; set; }
}

internal sealed class ReceivedEmailBody
{
    public string Plain { get; set; }

    public string Html { get; set; }
}

internal sealed class MailcatcherClient
{
    private readonly HttpClient httpClient;

    public string SmtpHost { get; }

    public int SmtpPort { get; }

    public MailcatcherClient(string apiHost, int apiPort, string smtpHost, int smtpPort)
    {
        SmtpHost = smtpHost;
        SmtpPort = smtpPort;

        httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://{apiHost}:{apiPort}")
        };
    }

    public Task<ReceivedEmail[]> GetMessagesAsync(
        CancellationToken ct = default)
    {
        return httpClient.GetFromJsonAsync<ReceivedEmail[]>("/messages", ct);
    }

    public async Task<ReceivedEmailBody> GetBodyAsync(int id,
        CancellationToken ct = default)
    {
        var responses = await Task.WhenAll(
            httpClient.GetStringAsync($"/messages/{id}.html", ct),
            httpClient.GetStringAsync($"/messages/{id}.plain", ct));

        return new ReceivedEmailBody { Html = responses[0], Plain = responses[1] };
    }
}
