// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http.Json;

#pragma warning disable MA0048 // File name must match type name

namespace TestSuite.ApiTests
{
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

        public string Host { get; }

        public MailcatcherClient(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                host = "localhost";
            }

            httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://{host}:1080")
            };
            Host = host;
        }

        public Task<ReceivedEmail[]> GetMessagesAsync()
        {
            return httpClient.GetFromJsonAsync<ReceivedEmail[]>("/messages");
        }

        public async Task<ReceivedEmailBody> GetBodyAsync(int id)
        {
            var responses = await Task.WhenAll(
                httpClient.GetStringAsync($"/messages/{id}.html"),
                httpClient.GetStringAsync($"/messages/{id}.plain"));

            return new ReceivedEmailBody { Html = responses[0], Plain = responses[1] };
        }
    }
}
