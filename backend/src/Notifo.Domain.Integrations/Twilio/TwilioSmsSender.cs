// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using ISmsClient = Twilio.Clients.ITwilioRestClient;

namespace Notifo.Domain.Integrations.Twilio;

public sealed class TwilioSmsSender : ISmsSender, IIntegrationHook
{
    private readonly ISmsCallback callback;
    private readonly ISmsClient client;
    private readonly string phoneNumber;

    public string Name => "Twilio SMS";

    public TwilioSmsSender(ISmsCallback callback, ISmsClient client, string phoneNumber)
    {
        this.callback = callback;
        this.client = client;
        this.phoneNumber = phoneNumber;
    }

    public async Task<SmsResult> SendAsync(SmsMessage message,
        CancellationToken ct)
    {
        var (_, to, text, _) = message;
        try
        {
            var result = await MessageResource.CreateAsync(
                ConvertPhoneNumber(to), null,
                ConvertPhoneNumber(phoneNumber), null,
                text,
                statusCallback: new Uri(BuildCallbackUrl(message)), client: client);

            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Twilio_Error, to, result.ErrorMessage);

                throw new DomainException(errorMessage);
            }

            return SmsResult.Sent;
        }
        catch (Exception ex)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Twilio_ErrorUnknown, to);

            throw new DomainException(errorMessage, ex);
        }
    }

    private static string BuildCallbackUrl(SmsMessage request)
    {
        return request.CallbackUrl.AppendQueries(RequestKeys.ReferenceValue, request.NotificationId, RequestKeys.ReferenceNumber, request.To);
    }

    private static PhoneNumber ConvertPhoneNumber(string number)
    {
        number = number.TrimStart('0');

        if (!number.StartsWith('+'))
        {
            number = $"+{number}";
        }

        return new PhoneNumber(number);
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
