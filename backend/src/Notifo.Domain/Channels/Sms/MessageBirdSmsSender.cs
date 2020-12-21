// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Integrations.MessageBird;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Sms
{
    public sealed class MessageBirdSmsSender : ISmsSender
    {
        private readonly List<Func<string, SmsResult, Task>> handlers = new List<Func<string, SmsResult, Task>>();
        private readonly MessageBirdClient smsClient;
        private readonly ISmsUrl smsUrl;

        public MessageBirdSmsSender(MessageBirdClient smsClient, ISmsUrl smsUrl)
        {
            this.smsClient = smsClient;
            this.smsUrl = smsUrl;
        }

        public Task RegisterAsync(Func<string, SmsResult, Task> handler)
        {
            Guard.NotNull(handler, nameof(handler));

            handlers.Add(handler);

            return Task.CompletedTask;
        }

        public async Task HandleStatusAsync(HttpContext httpContext)
        {
            if (handlers.Count == 0)
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
                    foreach (var handler in handlers)
                    {
                        await handler(status.Reference, smsResult);
                    }
                }
            }
        }

        public async Task<SmsResult> SendAsync(string to, string body, string? token, CancellationToken ct = default)
        {
            try
            {
                var sms = new MessageBirdSmsMessage(to, body, token, smsUrl.WebhookUrl());

                var response = await smsClient.SendSmsAsync(sms, ct);

                if (response.Recipients.TotalSentCount != 1)
                {
                    throw new DomainException($"Failed to send sms to {to}.");
                }

                return SmsResult.Sent;
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Failed to send sms to {to}: {ex.Message}.", ex);
            }
        }
    }
}
