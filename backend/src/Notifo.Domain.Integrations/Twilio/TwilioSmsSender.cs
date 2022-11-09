// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Apps;
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
    private readonly ISmsCallback smsCallback;
    private readonly ISmsUrl smsUrl;
    private readonly string phoneNumber;
    private readonly string integrationId;

    public string Name => "Twilio SMS";

    public TwilioSmsSender(
        string phoneNumber,
        ITwilioRestClient twilioClient,
        ISmsCallback smsCallback,
        ISmsUrl smsUrl,
        string integrationId)
    {
        this.twilioClient = twilioClient;
        this.smsCallback = smsCallback;
        this.smsUrl = smsUrl;
        this.phoneNumber = phoneNumber;
        this.integrationId = integrationId;
    }

    public async Task<SmsResult> SendAsync(App app, string to, string body, string reference,
        CancellationToken ct = default)
    {
        try
        {
            var callbackUrl = smsUrl.SmsWebhookUrl(app.Id, integrationId, new Dictionary<string, string>
            {
                ["reference"] = reference,
                ["reference_number"] = to
            });

            var result = await MessageResource.CreateAsync(
                ConvertPhoneNumber(to), null,
                ConvertPhoneNumber(phoneNumber), null,
                body,
                statusCallback: new Uri(callbackUrl), client: twilioClient);

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

    public Task HandleCallbackAsync(App app, HttpContext httpContext)
    {
        var request = httpContext.Request;

        var status = request.Form["MessageStatus"].ToString();

        var referenceString = request.Query["reference"].ToString();
        var referenceNumber = request.Query["reference_number"].ToString();

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
