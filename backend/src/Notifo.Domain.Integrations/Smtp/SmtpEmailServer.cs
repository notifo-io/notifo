// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MailKit.Net.Smtp;
using Microsoft.Extensions.ObjectPool;
using MimeKit;
using MimeKit.Text;
using Notifo.Domain.Channels.Email;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.Smtp;

public class SmtpEmailServer : IEmailSender, IDisposable
{
    private readonly ObjectPool<SmtpClient> clientPool;
    private readonly SmtpOptions options;

    public string Name => "SMTP";

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

    public async Task SendAsync(EmailRequest request,
        CancellationToken ct = default)
    {
        var smtpClient = clientPool.Get();
        try
        {
            await EnsureConnectedAsync(smtpClient);

            var smtpMessage = new MimeMessage();

            smtpMessage.From.Add(new MailboxAddress(
                request.FromName,
                request.FromEmail));

            smtpMessage.To.Add(new MailboxAddress(
                request.ToName,
                request.ToEmail));

            var hasHtml = !string.IsNullOrWhiteSpace(request.BodyHtml);
            var hasText = !string.IsNullOrWhiteSpace(request.BodyText);

            if (hasHtml && hasText)
            {
                smtpMessage.Body = new MultipartAlternative
                {
                    new TextPart(TextFormat.Plain)
                    {
                        Text = request.BodyText
                    },

                    new TextPart(TextFormat.Html)
                    {
                        Text = request.BodyHtml
                    }
                };
            }
            else if (hasHtml)
            {
                smtpMessage.Body = new TextPart(TextFormat.Html)
                {
                    Text = request.BodyHtml
                };
            }
            else if (hasText)
            {
                smtpMessage.Body = new TextPart(TextFormat.Plain)
                {
                    Text = request.BodyText
                };
            }
            else
            {
                ThrowHelper.InvalidOperationException("Cannot send email without text body or html body");
                return;
            }

            smtpMessage.Subject = request.Subject;

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

        if (string.IsNullOrWhiteSpace(options.Username) ||
            string.IsNullOrWhiteSpace(options.Password))
        {
            return;
        }

        if (!smtpClient.IsAuthenticated)
        {
            await smtpClient.AuthenticateAsync(options.Username, options.Password);
        }
    }
}
