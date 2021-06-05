// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Channels.Email;

namespace Notifo.Domain.Integrations.Mailjet
{
    public sealed class MailjetEmailSender : IEmailSender
    {
        private readonly MailjetEmailServer server;
        private readonly string fromEmail;
        private readonly string fromName;

        public MailjetEmailSender(MailjetEmailServer server,
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
