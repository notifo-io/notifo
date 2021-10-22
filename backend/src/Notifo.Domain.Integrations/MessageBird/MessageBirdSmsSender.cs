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
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class MessageBirdSmsSender : ISmsSender
    {
        private readonly MessageBirdClient messageBirdClient;
        private readonly ISmsCallback smsCallback;
        private readonly ISmsUrl smsUrl;
        private readonly string integrationId;

        public MessageBirdSmsSender(
            MessageBirdClient messageBirdClient,
            ISmsCallback smsCallback,
            ISmsUrl smsUrl,
            string integrationId)
        {
            this.messageBirdClient = messageBirdClient;
            this.smsCallback = smsCallback;
            this.smsUrl = smsUrl;
            this.integrationId = integrationId;
        }

        public async Task HandleCallbackAsync(App app, HttpContext httpContext)
        {
            var status = await messageBirdClient.ParseStatusAsync(httpContext);

            if (status.Reference != null)
            {
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

                if (result != SmsResult.Unknown)
                {
                    await smsCallback.HandleCallbackAsync(status.Recipient, status.Reference, result, httpContext.RequestAborted);
                }
            }
        }

        public async Task<SmsResult> SendAsync(App app, string to, string body, string token,
            CancellationToken ct = default)
        {
            try
            {
                var callbackUrl = smsUrl.SmsWebhookUrl(app.Id, integrationId);

                var sms = new MessageBirdSmsMessage(to, body, token, callbackUrl);

                var response = await messageBirdClient.SendSmsAsync(sms, ct);

                if (response.Recipients.TotalSentCount != 1)
                {
                    var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.MessageBird_ErrorUnknown, to);

                    throw new DomainException(errorMessage);
                }

                return SmsResult.Sent;
            }
            catch (ArgumentException ex)
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.MessageBird_Error, to, ex.Message);

                throw new DomainException(errorMessage);
            }
        }
    }
}
