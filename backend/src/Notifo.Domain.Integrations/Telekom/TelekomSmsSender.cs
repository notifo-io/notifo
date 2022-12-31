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
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.Telekom;

public sealed class TelekomSmsSender : ISmsSender, IIntegrationHook
{
    private readonly ISmsCallback callback;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string apikey;
    private readonly string phoneNumber;

    public string Name => "Telekom SMS";

    public TelekomSmsSender(
        ISmsCallback callback,
        IHttpClientFactory httpClientFactory,
        string apikey,
        string phoneNumber)
    {
        this.apikey = apikey;
        this.phoneNumber = phoneNumber;
        this.callback = callback;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<SmsResult> SendAsync(SmsMessage message,
        CancellationToken ct)
    {
        var (_, to, text, _) = message;
        try
        {
            var httpClient = httpClientFactory.CreateClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                [RequestKeys.From] = ConvertPhoneNumber(phoneNumber),
                [RequestKeys.To] = ConvertPhoneNumber(to),
                [RequestKeys.Body] = text,
                [RequestKeys.StatusCallback] = BuildCallbackUrl(message),
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

    private static string BuildCallbackUrl(SmsMessage message)
    {
        return message.CallbackUrl.AppendQueries(RequestKeys.ReferenceValue, message.NotificationId, RequestKeys.ReferenceNumber, message.To);
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

    public Task HandleRequestAsync(AppContext app, HttpContext httpContext)
    {
        var status = httpContext.Request.Form[RequestKeys.MessageStatus].ToString();

        var referenceString = httpContext.Request.Query[RequestKeys.ReferenceValue].ToString();
        var referenceNumber = httpContext.Request.Query[RequestKeys.ReferenceNumber].ToString();

        // If the reference is not a valid guid (notification-id), something just went wrong.
        if (!Guid.TryParse(referenceString, out var notificationId))
        {
            return Task.CompletedTask;
        }

        var result = ParseStatus(status);

        if (result == default)
        {
            return Task.CompletedTask;
        }

        return callback.HandleCallbackAsync(this, notificationId, referenceNumber, result);

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
