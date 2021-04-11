// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Channels.Email;

namespace Notifo.Domain.Integrations.Smtp
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpEmailServer server;
        private readonly string fromEmail;
        private readonly string fromName;

        public SmtpEmailSender(SmtpEmailServer server,
            string fromEmail,
            string fromName)
        {
            this.server = server;

            this.fromEmail = fromEmail;
            this.fromName = fromName;
        }

        public Task SendAsync(EmailMessage message, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(message.FromEmail))
            {
                message.FromEmail = fromEmail;
            }

            if (string.IsNullOrWhiteSpace(message.FromName))
            {
                message.FromName = fromName;
            }

            return server.SendAsync(message, ct);
        }
    }
}
