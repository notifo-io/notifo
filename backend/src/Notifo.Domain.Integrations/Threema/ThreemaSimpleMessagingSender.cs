// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;

namespace Notifo.Domain.Integrations.Threema;

public sealed class ThreemaSimpleMessagingSender : IMessagingSender
{
    private const string ThreemaPhoneNumber = nameof(ThreemaPhoneNumber);
    private const string ThreemaEmail = nameof(ThreemaEmail);
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string apiIdentity;
    private readonly string apiSecret;

    public string Name => "Threema";

    public ThreemaSimpleMessagingSender(
        IHttpClientFactory httpClientFactory,
        string apiIdentity,
        string apiSecret)
    {
        this.httpClientFactory = httpClientFactory;
        this.apiIdentity = apiIdentity;
        this.apiSecret = apiSecret;
    }

    public void AddTargets(MessagingTargets targets, UserContext user)
    {
        var phoneNumber = user.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            targets[ThreemaPhoneNumber] = phoneNumber;
        }

        var email = user.EmailAddress;

        if (!string.IsNullOrWhiteSpace(email))
        {
            targets[ThreemaEmail] = email;
        }
    }

    public async Task<MessagingResult> SendAsync(MessagingMessage message,
        CancellationToken ct)
    {
        var httpClient = httpClientFactory.CreateClient();

        Exception? exception = null;

        if (message.Targets.TryGetValue(ThreemaPhoneNumber, out var phoneNumber))
        {
            try
            {
                if (await SendAsync(httpClient, "phone", phoneNumber, message.Text, ct))
                {
                    return MessagingResult.Delivered;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        if (message.Targets.TryGetValue(ThreemaEmail, out var email))
        {
            try
            {
                if (await SendAsync(httpClient, "email", email, message.Text, ct))
                {
                    return MessagingResult.Delivered;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        if (exception != null)
        {
            throw exception;
        }

        return MessagingResult.Skipped;
    }

    private async Task<bool> SendAsync(HttpClient httpClient, string toKey, string toValue, string text,
        CancellationToken ct)
    {
        // Read the API documentation: https://gateway.threema.ch/de/developer/api
        const string Url = "https://msgapi.threema.ch/send_simple";

        var parameters = new[]
        {
            new KeyValuePair<string, string>("secret", apiSecret),
            new KeyValuePair<string, string>("from", apiIdentity),
            new KeyValuePair<string, string>("text", text),
            new KeyValuePair<string, string>(toKey, toValue)
        };

        var form = new FormUrlEncodedContent(parameters!);

        var response = await httpClient.PostAsync(Url, form, ct);

        // BadRequest (400) is returned when the to parameter is invalid.
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();

        return true;
    }
}
