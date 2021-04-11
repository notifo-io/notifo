// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.ObjectPool;
using MimeKit;
using MimeKit.Text;
using Notifo.Domain.Channels.Email;

namespace Notifo.Domain.Integrations.Smtp
{
    public class SmtpEmailServer : IDisposable
    {
        private readonly ObjectPool<SmtpClient> clientPool;
        private readonly SmtpOptions options;

        public SmtpEmailServer(SmtpOptions options)
        {
            this.options = options;

            clientPool = new DefaultObjectPoolProvider().Create(new DefaultPooledObjectPolicy<SmtpClient>());
        }

        public void Dispose()
        {
            if (clientPool is IDisposable disposable)
            {
                disposable.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public async Task SendAsync(EmailMessage message, string fromName, string fromEmail, CancellationToken ct)
        {
            var smtpClient = clientPool.Get();
            try
            {
                await EnsureConnectedAsync(smtpClient);

                var smtpMessage = new MimeMessage();

                smtpMessage.From.Add(new MailboxAddress(
                    fromName,
                    fromEmail));

                smtpMessage.To.Add(new MailboxAddress(
                    message.RecipientName,
                    message.RecipientEmail));

                var hasHtml = !string.IsNullOrWhiteSpace(message.BodyHtml);
                var hasText = !string.IsNullOrWhiteSpace(message.BodyText);

                if (hasHtml && hasText)
                {
                    smtpMessage.Body = new MultipartAlternative
                    {
                        new TextPart(TextFormat.Plain)
                        {
                            Text = message.BodyText
                        },

                        new TextPart(TextFormat.Html)
                        {
                            Text = message.BodyHtml
                        }
                    };
                }
                else if (hasHtml)
                {
                    smtpMessage.Body = new TextPart(TextFormat.Html)
                    {
                        Text = message.BodyHtml
                    };
                }
                else if (hasText)
                {
                    smtpMessage.Body = new TextPart(TextFormat.Plain)
                    {
                        Text = message.BodyText
                    };
                }
                else
                {
                    throw new InvalidOperationException("Cannot send email without text body or html body");
                }

                smtpMessage.Subject = message.Subject;

                await smtpClient.SendAsync(smtpMessage, ct);
            }
            finally
            {
                clientPool.Return(smtpClient);
            }
        }

        private async Task EnsureConnectedAsync(SmtpClient smtpClient)
        {
            if (!smtpClient.IsConnected)
            {
                await smtpClient.ConnectAsync(options.Host, options.HostPort);
            }

            if (!smtpClient.IsAuthenticated)
            {
                await smtpClient.AuthenticateAsync(options.Username, options.Password);
            }
        }
    }
}
