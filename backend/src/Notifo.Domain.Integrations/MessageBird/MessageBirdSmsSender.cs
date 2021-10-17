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
                var smsResult = default(SmsResult);

                switch (status.Status)
                {
                    case MessageBirdStatus.Delivered:
                        smsResult = SmsResult.Delivered;
                        break;
                    case MessageBirdStatus.Delivery_Failed:
                        smsResult = SmsResult.Failed;
                        break;
                    case MessageBirdStatus.Sent:
                        smsResult = SmsResult.Sent;
                        break;
                }

                if (smsResult != SmsResult.Unknown)
                {
                    var response = new SmsResponse
                    {
                        Status = smsResult,
                        Reference = status.Reference,
                        Recipient = status.Recipient
                    };

                    await smsCallback.HandleCallbackAsync(response, httpContext.RequestAborted);
                }
            }
        }

        public async Task<SmsResult> SendAsync(App app, string to, string body, string? token = null,
            CancellationToken ct = default)
        {
            try
            {
                var sms = new MessageBirdSmsMessage(to, body, token, smsUrl.SmsWebhookUrl(app.Id, integrationId));

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
