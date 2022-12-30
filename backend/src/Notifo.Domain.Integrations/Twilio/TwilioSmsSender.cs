// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Notifo.Domain.Integrations.Twilio;

public sealed class TwilioSmsSender : ISmsSender
{
    private readonly ITwilioRestClient twilioClient;
    private readonly string phoneNumber;

    public string Name => "Twilio SMS";

    public TwilioSmsSender(ITwilioRestClient twilioClient, string phoneNumber)
    {
        this.twilioClient = twilioClient;
        this.phoneNumber = phoneNumber;
    }

    public async Task<SmsResult> SendAsync(SmsRequest request,
        CancellationToken ct = default)
    {
        var (to, message, callbackUrl) = request;
        try
        {
            var result = await MessageResource.CreateAsync(
                ConvertPhoneNumber(to), null,
                ConvertPhoneNumber(phoneNumber), null,
                message,
                statusCallback: new Uri(callbackUrl!), client: twilioClient);

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

    private static PhoneNumber ConvertPhoneNumber(string number)
    {
        number = number.TrimStart('0');

        if (!number.StartsWith('+'))
        {
            number = $"+{number}";
        }

        return new PhoneNumber(number);
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
