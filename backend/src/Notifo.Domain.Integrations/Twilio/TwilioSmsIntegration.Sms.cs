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

namespace Notifo.Domain.Integrations.Twilio;

public sealed partial class TwilioSmsIntegration : ISmsSender, IIntegrationHook
{
    public async Task<DeliveryResult> SendAsync(IntegrationContext context, SmsMessage message,
        CancellationToken ct)
    {
        var accountSid = AccountSidProperty.GetString(context.Properties);
        var accountToken = AuthTokenProperty.GetString(context.Properties);
        var phoneNumber = PhoneNumberProperty.GetNumber(context.Properties);

        var client = clientPool.GetServer(accountSid, accountToken);
        try
        {
            var to = message.To;

            var result = await MessageResource.CreateAsync(
                ConvertPhoneNumber(to), null,
                ConvertPhoneNumber(phoneNumber), null,
                message.Text,
                statusCallback: new Uri(BuildCallbackUrl(context, message)), client: client);

            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Twilio_Error, to, result.ErrorMessage);

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

    private static string BuildCallbackUrl(IntegrationContext context, SmsMessage request)
    {
        return context.WebhookUrl.AppendQueries(RequestKeys.Reference, request.TrackingToken);
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

        return context.UpdateStatusAsync(reference, result);

        static DeliveryResult ParseStatus(string status)
        {
            switch (status)
            {
                case "sent":
                    return DeliveryResult.Sent;
                case "delivered":
                    return DeliveryResult.Handled;
                case "failed":
                case "undelivered":
                    return DeliveryResult.Failed();
                default:
                    return default;
            }
        }
    }
}
