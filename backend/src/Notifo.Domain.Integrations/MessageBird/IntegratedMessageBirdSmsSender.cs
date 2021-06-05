// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class IntegratedMessageBirdSmsSender : ISmsSender
    {
        private readonly ConcurrentBag<SmsHandler> handlers = new ConcurrentBag<SmsHandler>();
        private readonly MessageBirdClient smsClient;
        private readonly ISmsUrl smsUrl;

        public IntegratedMessageBirdSmsSender(MessageBirdClient smsClient, ISmsUrl smsUrl)
        {
            this.smsClient = smsClient;
            this.smsUrl = smsUrl;
        }

        public Task RegisterAsync(SmsHandler handler)
        {
            Guard.NotNull(handler, nameof(handler));

            handlers.Add(handler);

            return Task.CompletedTask;
        }

        public async Task HandleStatusAsync(HttpContext httpContext)
        {
            if (handlers.IsEmpty)
            {
                return;
            }

            var status = smsClient.ParseStatus(httpContext);

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

                    foreach (var handler in handlers)
                    {
                        await handler(response);
                    }
                }
            }
        }

        public async Task<SmsResult> SendAsync(string to, string body, string? token, CancellationToken ct)
        {
            try
            {
                var sms = new MessageBirdSmsMessage(to, body, token, smsUrl.WebhookUrl());

                var response = await smsClient.SendSmsAsync(sms, ct);

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
