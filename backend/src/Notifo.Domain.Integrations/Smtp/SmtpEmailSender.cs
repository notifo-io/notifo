// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.Smtp;

public sealed class SmtpEmailSender : IEmailSender
{
    private const int Attempts = 5;
    private readonly Func<SmtpEmailServer> server;
    private readonly string fromEmail;
    private readonly string fromName;

    public string Name => "SMTP";

    public SmtpEmailSender(Func<SmtpEmailServer> server,
        string fromEmail,
        string fromName)
    {
        this.server = server;

        this.fromEmail = fromEmail;
        this.fromName = fromName;
    }

    public async Task SendAsync(EmailMessage message,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(message.FromEmail))
        {
            message.FromEmail = fromEmail;
        }

        if (string.IsNullOrWhiteSpace(message.FromName))
        {
            message.FromName = fromName;
        }

        // Try a few attempts to get a non-disposed server instance.
        for (var i = 1; i <= Attempts; i++)
        {
            try
            {
                await server().SendAsync(message, ct);
                break;
            }
            catch (ObjectDisposedException)
            {
                if (i == Attempts)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                var error = string.Format(CultureInfo.InvariantCulture, Texts.SMTP_Exception, ex.Message);

                throw new DomainException(error, ex);
            }
        }
    }
}
