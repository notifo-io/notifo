// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;

namespace Notifo.Domain.Integrations.Threema;

public sealed partial class ThreemaSimpleIntegration : IMessagingSender
{
    private const string ThreemaPhoneNumber = nameof(ThreemaPhoneNumber);
    private const string ThreemaEmail = nameof(ThreemaEmail);

    public void AddTargets(IDictionary<string, string> targets, UserInfo user)
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

    public async Task<DeliveryResult> SendAsync(IntegrationContext context, MessagingMessage message,
        CancellationToken ct)
    {
        var apiIdentity = ApiIdentity.GetString(context.Properties);
        var apiSecret = ApiSecret.GetString(context.Properties);

        var httpClient = httpClientFactory.CreateClient();

        Exception? exception = null;

        if (message.Targets.TryGetValue(ThreemaPhoneNumber, out var phoneNumber))
        {
            try
            {
                if (await SendAsync(httpClient, apiSecret, apiIdentity, "phone", phoneNumber, message.Text, ct))
                {
                    return DeliveryResult.Handled;
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
                if (await SendAsync(httpClient, apiSecret, apiIdentity, "email", email, message.Text, ct))
                {
                    return DeliveryResult.Handled;
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

        return DeliveryResult.Skipped();
    }

    private static async Task<bool> SendAsync(HttpClient httpClient, string apiSecret, string apiIdentity, string toKey, string toValue, string text,
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
