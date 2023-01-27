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
    private readonly string webhookUrl;

    public string Name => "Twilio SMS";

    public TwilioSmsSender(ISmsCallback callback, ISmsClient client, string phoneNumber, string webhookUrl)
    {
        this.callback = callback;
        this.client = client;
        this.phoneNumber = phoneNumber;
        this.webhookUrl = webhookUrl;
    }

    public async Task<DeliveryResult> SendAsync(SmsMessage message,
        CancellationToken ct)
    {
        try
        {
            var result = await MessageResource.CreateAsync(
                ConvertPhoneNumber(message.To), null,
                ConvertPhoneNumber(phoneNumber), null,
                message.Text,
                statusCallback: new Uri(BuildCallbackUrl(message)), client: client);

            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Twilio_Error, message.To, result.ErrorMessage);

                throw new DomainException(errorMessage);
            }

            return DeliveryResult.Sent;
        }
        catch (Exception ex)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Twilio_ErrorUnknown, message.To);

            throw new DomainException(errorMessage, ex);
        }
    }

    private string BuildCallbackUrl(SmsMessage request)
    {
        return webhookUrl.AppendQueries(RequestKeys.ReferenceValue, request.NotificationId, RequestKeys.ReferenceNumber, request.To);
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
