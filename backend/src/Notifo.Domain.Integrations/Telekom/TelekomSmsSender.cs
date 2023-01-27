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
    private readonly string webhookUrl;

    public string Name => "Telekom SMS";

    public TelekomSmsSender(
        ISmsCallback callback,
        IHttpClientFactory httpClientFactory,
        string apikey,
        string phoneNumber,
        string webhookUrl)
    {
        this.apikey = apikey;
        this.phoneNumber = phoneNumber;
        this.webhookUrl = webhookUrl;
        this.callback = callback;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<DeliveryResult> SendAsync(SmsMessage message,
        CancellationToken ct)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                [RequestKeys.From] = ConvertPhoneNumber(phoneNumber),
                [RequestKeys.To] = ConvertPhoneNumber(message.To),
                [RequestKeys.Body] = message.Text,
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
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Telekom_Error, message.To, result.ErrorMessage);

                throw new DomainException(errorMessage);
            }

            return DeliveryResult.Sent;
        }
        catch (Exception ex)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Telekom_ErrorUnknown, message.To);

            throw new DomainException(errorMessage, ex);
        }
    }

    private string BuildCallbackUrl(SmsMessage message)
    {
        return webhookUrl.AppendQueries(RequestKeys.ReferenceValue, message.NotificationId, RequestKeys.ReferenceNumber, message.To);
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

    public Task HandleRequestAsync(IntegrationContext context, HttpContext httpContext)
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

        static DeliveryResult ParseStatus(string status)
        {
            switch (status)
            {
                case "sent":
                    return DeliveryResult.Sent;
                case "delivered":
                    return DeliveryResult.Delivered;
                case "failed":
                case "undelivered":
                    return DeliveryResult.Failed;
                default:
                    return default;
            }
        }
    }
}
