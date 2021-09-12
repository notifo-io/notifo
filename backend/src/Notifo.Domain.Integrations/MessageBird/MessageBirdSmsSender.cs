// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class MessageBirdSmsSender : ISmsSender
    {
        private readonly MessageBirdClient smsClient;

        public MessageBirdSmsSender(MessageBirdClient smsClient)
        {
            this.smsClient = smsClient;
        }

        public Task RegisterAsync(SmsHandler handler)
        {
            return Task.CompletedTask;
        }

        public Task HandleStatusAsync(HttpContext httpContext)
        {
            return Task.CompletedTask;
        }

        public async Task<SmsResult> SendAsync(string to, string body, string? token = null,
            CancellationToken ct = default)
        {
            try
            {
                var sms = new MessageBirdSmsMessage(to, body, token);

                var response = await smsClient.SendSmsAsync(sms, ct);

                if (response.Recipients.TotalSentCount != 1)
                {
                    throw new DomainException($"Failed to send sms to {to}.");
                }

                return SmsResult.Delivered;
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Failed to send sms to {to}: {ex.Message}.", ex);
            }
        }
    }
}
