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
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.Telekom;

public sealed class TelekomSmsSender : ISmsSender
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ISmsCallback smsCallback;
    private readonly ISmsUrl smsUrl;
    private readonly string apikey;
    private readonly string phoneNumber;
    private readonly string integrationId;

    public string Name => "Telekom SMS";

    public TelekomSmsSender(
        IHttpClientFactory httpClientFactory,
        ISmsCallback smsCallback,
        ISmsUrl smsUrl,
        string apikey,
        string phoneNumber,
        string integrationId)
    {
        this.apikey = apikey;
        this.phoneNumber = phoneNumber;
        this.httpClientFactory = httpClientFactory;
        this.smsCallback = smsCallback;
        this.smsUrl = smsUrl;
        this.integrationId = integrationId;
    }

    public async Task<SmsResult> SendAsync(App app, string to, string body, string reference,
        CancellationToken ct = default)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [RequestKeys.From] = ConvertPhoneNumber(phoneNumber),
                [RequestKeys.To] = ConvertPhoneNumber(to),
                [RequestKeys.Body] = body,
                [RequestKeys.StatusCallback] = BuildCallbackUrl(app, to, reference),
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://developer-api.telekom.com/vms/Messages.json")
            {
                Content = content
            };

            request.Headers.TryAddWithoutValidation("Authorization", apikey);

            var response = await httpClient.SendAsync(request, ct);

            var x = await response.Content.ReadAsStringAsync();
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

    private string BuildCallbackUrl(App app, string to, string reference)
    {
        var query = new Dictionary<string, string>
        {
            [RequestKeys.ReferenceValue] = reference,
            [RequestKeys.ReferenceNumber] = to
        };

        return smsUrl.SmsWebhookUrl(app.Id, integrationId, query);
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

    public Task HandleCallbackAsync(App app, HttpContext httpContext)
    {
        var request = httpContext.Request;

        var status = request.Form[RequestKeys.MessageStatus].ToString();

        var referenceString = request.Query[RequestKeys.ReferenceValue].ToString();
        var referenceNumber = request.Query[RequestKeys.ReferenceNumber].ToString();

        if (!Guid.TryParse(referenceString, out var notificationId))
        {
            return Task.CompletedTask;
        }

        var result = default(SmsResult);

        switch (status)
        {
            case "sent":
                result = SmsResult.Sent;
                break;
            case "delivered":
                result = SmsResult.Delivered;
                break;
            case "failed":
            case "undelivered":
                result = SmsResult.Failed;
                break;
        }

        if (result == SmsResult.Unknown)
        {
            return Task.CompletedTask;
        }

        var callback = new SmsCallbackResponse(notificationId, referenceNumber, result);

        return smsCallback.HandleCallbackAsync(this, callback, httpContext.RequestAborted);
    }
}
