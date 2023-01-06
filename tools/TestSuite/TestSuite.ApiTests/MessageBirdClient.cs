// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http.Headers;
using System.Net.Http.Json;

#pragma warning disable MA0048 // File name must match type name

namespace TestSuite.ApiTests;

public sealed class MessageBirdMessages
{
    public MessageBirdMessage[] Items { get; set; }
}

public sealed class MessageBirdMessage
{
    public string Body { get; set; }

    public MessageBirdRecipients Recipients { get; set; }
}

public sealed class MessageBirdRecipients
{
    public MessageBirdRecipient[] Items { get; set; }
}

public sealed class MessageBirdRecipient
{
    public string Originator { get; set; }

    public string Status { get; set; }
}

public sealed class MessageBirdClient
{
    private readonly HttpClient httpClient;

    public MessageBirdClient(string accessKey)
    {
        httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://rest.messagebird.com")
        };

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("AccessKey", accessKey);
    }

    public Task<MessageBirdMessages> GetMessagesAsync(int limit)
    {
        return httpClient.GetFromJsonAsync<MessageBirdMessages>($"/messages?limit={limit}")!;
    }
}
