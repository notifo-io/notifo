// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Notifo.Domain.Integrations.Twilio
{
    public sealed class TwilioSmsSender : ISmsSender
    {
        private readonly ITwilioRestClient twilioClient;
        private readonly ISmsCallback smsCallback;
        private readonly ISmsUrl smsUrl;
        private readonly string phoneNumber;
        private readonly string integrationId;

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

        public async Task HandleCallbackAsync(App app, HttpContext httpContext)
        {
            var request = httpContext.Request;

            var status = request.Form["MessageStatus"].ToString();

            var reference = request.Query["reference"].ToString();
            var referenceNumber = request.Query["referenceNumber"].ToString();

            var smsResult = default(SmsResult);

            switch (status)
            {
                case "sent":
                    smsResult = SmsResult.Sent;
                    break;
                case "delivered":
                    smsResult = SmsResult.Delivered;
                    break;
                case "failed":
                case "undelivered":
                    smsResult = SmsResult.Failed;
                    break;
            }

            if (smsResult != SmsResult.Unknown)
            {
                var response = new SmsResponse
                {
                    Status = smsResult,
                    Reference = reference,
                    ReferenceNumber = referenceNumber
                };

                await smsCallback.HandleCallbackAsync(response, httpContext.RequestAborted);
            }
        }

        public async Task<SmsResult> SendAsync(App app, string to, string body, string reference,
            CancellationToken ct = default)
        {
            try
            {
                var callbackUrl = smsUrl.SmsWebhookUrl(app.Id, integrationId, new System.Collections.Generic.Dictionary<string, string>
                {
                    ["reference"] = reference,
                    ["referenceNumber"] = to
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
    }
}
