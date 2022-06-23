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
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class MessageBirdSmsSender : ISmsSender
    {
        private readonly IMessageBirdClient messageBirdClient;
        private readonly ISmsCallback smsCallback;
        private readonly ISmsUrl smsUrl;
        private readonly string integrationId;
        private readonly string phoneNumber;
        private readonly Dictionary<string, string>? phoneNumbers;

        public string Name => "MessageBird SMS";

        public MessageBirdSmsSender(
            IMessageBirdClient messageBirdClient,
            ISmsCallback smsCallback,
            ISmsUrl smsUrl,
            string integrationId,
            string phoneNumber,
            Dictionary<string, string>? phoneNumbers)
        {
            this.messageBirdClient = messageBirdClient;
            this.smsCallback = smsCallback;
            this.smsUrl = smsUrl;
            this.integrationId = integrationId;
            this.phoneNumber = phoneNumber;
            this.phoneNumbers = phoneNumbers;
        }

        public async Task<SmsResult> SendAsync(App app, string to, string body, string token,
            CancellationToken ct = default)
        {
            try
            {
                // The callback URL is used to get delivery status.
                var callbackUrl = smsUrl.SmsWebhookUrl(app.Id, integrationId);

                // Call the phone number and use a local phone number for the user.
                var sms = new SmsMessage(GetOriginator(to), to, body, token, callbackUrl);

                var response = await messageBirdClient.SendSmsAsync(sms, ct);

                // Usually an error is received by the error response in the client, but in some cases it was not working properly.
                if (response.Recipients.TotalSentCount != 1)
                {
                    var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.MessageBird_ErrorUnknown, to);

                    throw new DomainException(errorMessage);
                }

                // We get the status asynchronously via webhook, therefore we tell the channel not mark the process as completed.
                return SmsResult.Sent;
            }
            catch (ArgumentException ex)
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.MessageBird_Error, to, ex.Message);

                throw new DomainException(errorMessage);
            }
        }

        private string GetOriginator(string to)
        {
            if (phoneNumbers?.Count > 0 && to.Length > 2)
            {
                // Use the country code of the phone number to not look as a spam SMS.
                var countryCode = to[..2];

                if (phoneNumbers.TryGetValue(countryCode, out var originator))
                {
                    return originator;
                }
            }

            return phoneNumber;
        }

        public async Task HandleCallbackAsync(App app, HttpContext httpContext)
        {
            var status = await messageBirdClient.ParseSmsWebhookAsync(httpContext);

            // If the reference is not a valid guid (notification-id), something just went wrong.
            if (!Guid.TryParse(status.Reference, out var reference) || reference == default)
            {
                return;
            }

            var result = default(SmsResult);

            switch (status.Status)
            {
                case MessageBirdStatus.Delivered:
                    result = SmsResult.Delivered;
                    break;
                case MessageBirdStatus.Delivery_Failed:
                    result = SmsResult.Failed;
                    break;
                case MessageBirdStatus.Sent:
                    result = SmsResult.Sent;
                    break;
            }

            if (result == SmsResult.Unknown)
            {
                return;
            }

            var callback = new SmsCallbackResponse(reference, status.Recipient, result);

            await smsCallback.HandleCallbackAsync(this, callback, httpContext.RequestAborted);
        }
    }
}
