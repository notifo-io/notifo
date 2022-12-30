// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.Telekom;

public sealed class TelekomSmsSender : ISmsSender
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string apikey;
    private readonly string phoneNumber;

    public string Name => "Telekom SMS";

    public TelekomSmsSender(
        IHttpClientFactory httpClientFactory,
        string apikey,
        string phoneNumber)
    {
        this.apikey = apikey;
        this.phoneNumber = phoneNumber;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<SmsResult> SendAsync(SmsRequest request,
        CancellationToken ct = default)
    {
        var (to, message, callbackUrl) = request;
        try
        {
            var httpClient = httpClientFactory.CreateClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                [RequestKeys.From] = ConvertPhoneNumber(phoneNumber),
                [RequestKeys.To] = ConvertPhoneNumber(to),
                [RequestKeys.Body] = message,
                [RequestKeys.StatusCallback] = callbackUrl,
            });

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://developer-api.telekom.com/vms/Messages.json")
            {
                Content = content
            };

            httpRequest.Headers.TryAddWithoutValidation("Authorization", apikey);

            var response = await httpClient.SendAsync(httpRequest, ct);

            var result = await response.Content.ReadFromJsonAsync<Response>((JsonSerializerOptions?)null, ct);

            if (!string.IsNullOrWhiteSpace(result?.ErrorMessage))
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Telekom_Error, to, result.ErrorMessage);

                throw new DomainException(errorMessage);
            }

            return SmsResult.Sent;
        }
        catch (Exception ex)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Telekom_ErrorUnknown, to);

            throw new DomainException(errorMessage, ex);
        }
    }

    private static string ConvertPhoneNumber(string number)
    {
        number = number.TrimStart('0');

        if (!number.StartsWith('+'))
        {
            number = $"+{number}";
        }

        return number;
    }

    public ValueTask<SmsCallbackResponse> HandleCallbackAsync(HttpContext httpContext)
    {
        var status = httpContext.Request.Form[RequestKeys.MessageStatus].ToString();

        return new ValueTask<SmsCallbackResponse>(new SmsCallbackResponse(ParseStatus(status)));

        static SmsResult ParseStatus(string status)
        {
            switch (status)
            {
                case "sent":
                    return SmsResult.Sent;
                case "delivered":
                    return SmsResult.Delivered;
                case "failed":
                case "undelivered":
                    return SmsResult.Failed;
                default:
                    return default;
            }
        }
    }
}
