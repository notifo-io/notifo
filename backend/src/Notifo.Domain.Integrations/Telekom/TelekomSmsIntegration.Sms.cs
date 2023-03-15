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

public sealed partial class TelekomSmsIntegration : ISmsSender, IIntegrationHook
{
    public async Task<DeliveryResult> SendAsync(IntegrationContext context, SmsMessage message,
        CancellationToken ct)
    {
        var phoneNumberFrom = PhoneNumberProperty.GetString(context.Properties);
        var phoneNumberTo = message.To;

        var apiKey = ApiKeyProperty.GetString(context.Properties);
        try
        {
            var httpClient = httpClientFactory.CreateClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                [RequestKeys.From] = ConvertPhoneNumber(phoneNumberFrom),
                [RequestKeys.To] = ConvertPhoneNumber(phoneNumberTo),
                [RequestKeys.Body] = message.Text,
                [RequestKeys.StatusCallback] = BuildCallbackUrl(context, message),
            });

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://developer-api.telekom.com/vms/Messages.json")
            {
                Content = content
            };

            httpRequest.Headers.TryAddWithoutValidation("Authorization", apiKey);

            var response = await httpClient.SendAsync(httpRequest, ct);

            var result = await response.Content.ReadFromJsonAsync<Response>((JsonSerializerOptions?)null, ct);

            if (!string.IsNullOrWhiteSpace(result?.ErrorMessage))
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Telekom_Error, phoneNumberTo, result.ErrorMessage);

                throw new DomainException(errorMessage);
            }

            return DeliveryResult.Sent;
        }
        catch (Exception ex)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Telekom_ErrorUnknown, phoneNumberTo);

            throw new DomainException(errorMessage, ex);
        }
    }

    private static string BuildCallbackUrl(IntegrationContext context, SmsMessage message)
    {
        return context.WebhookUrl.AppendQueries(RequestKeys.Reference, message.TrackingToken);
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

    public Task HandleRequestAsync(IntegrationContext context, HttpContext httpContext,
        CancellationToken ct)
    {
        httpContext.Request.Query.TryGetValue(RequestKeys.Reference, out var referenceQuery);

        string? reference = referenceQuery;

        if (string.IsNullOrWhiteSpace(reference))
        {
            return Task.CompletedTask;
        }

        var status = httpContext.Request.Form[RequestKeys.MessageStatus].ToString();

        var result = ParseStatus(status);

        if (result == default)
        {
            return Task.CompletedTask;
        }

        return context.UpdateStatusAsync(reference, new DeliveryResult(result));

        static DeliveryStatus ParseStatus(string status)
        {
            switch (status)
            {
                case "sent":
                    return DeliveryStatus.Sent;
                case "delivered":
                    return DeliveryStatus.Handled;
                case "failed":
                case "undelivered":
                    return DeliveryStatus.Failed;
                default:
                    return default;
            }
        }
    }
}
