// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.ObjectPool;
using MimeKit;
using MimeKit.Text;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.Smtp;

public class SmtpEmailServer(SmtpOptions options) : IDisposable
{
    private readonly ObjectPool<SmtpClient> clientPool = new DefaultObjectPoolProvider().Create(new DefaultPooledObjectPolicy<SmtpClient>());

    public void Dispose()
    {
        if (clientPool is IDisposable disposable)
        {
            disposable.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    public async Task SendAsync(EmailMessage message,
        CancellationToken ct)
    {
        var smtpMessage = new MimeMessage();

        smtpMessage.From.Add(new MailboxAddress(
            message.FromName,
            message.FromEmail));

        smtpMessage.To.Add(new MailboxAddress(
            message.ToName,
            message.ToEmail));

        var hasHtml = !string.IsNullOrWhiteSpace(message.BodyHtml);
        var hasText = !string.IsNullOrWhiteSpace(message.BodyText);

        if (hasHtml && hasText)
        {
            smtpMessage.Body = new MultipartAlternative
            {
                new TextPart(TextFormat.Plain)
                {
                    Text = message.BodyText!
                },

                new TextPart(TextFormat.Html)
                {
                    Text = message.BodyHtml!
                }
            };
        }
        else if (hasHtml)
        {
            smtpMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = message.BodyHtml!
            };
        }
        else if (hasText)
        {
            smtpMessage.Body = new TextPart(TextFormat.Plain)
            {
                Text = message.BodyText!
            };
        }
        else
        {
            ThrowHelper.InvalidOperationException("Cannot send email without text body or html body");
            return;
        }

        smtpMessage.Subject = message.Subject;

        var smtpClient = clientPool.Get();
        try
        {
            try
            {
                await EnsureConnectedAsync(smtpClient, ct);

                await smtpClient.SendAsync(smtpMessage, ct);
            }
            catch (Exception ex) when (ex is IOException or SmtpProtocolException or ServiceNotConnectedException)
            {
                // The server might have closed the pooled connection in the meantime, therefore try again with a new connection.
                smtpClient.Dispose();
                smtpClient = new SmtpClient();

                await EnsureConnectedAsync(smtpClient, ct);

                await smtpClient.SendAsync(smtpMessage, ct);
            }

            clientPool.Return(smtpClient);
        }
        catch
        {
            // Do not return the client to the pool, because the connection might be broken.
            smtpClient.Dispose();
            throw;
        }
    }

    private async Task EnsureConnectedAsync(SmtpClient smtpClient,
        CancellationToken ct)
    {
        if (!smtpClient.IsConnected)
        {
            await smtpClient.ConnectAsync(options.HostName, options.HostPort, cancellationToken: ct);
        }

        if (string.IsNullOrWhiteSpace(options.Username) ||
            string.IsNullOrWhiteSpace(options.Password))
        {
            return;
        }

        if (!smtpClient.IsAuthenticated)
        {
            await smtpClient.AuthenticateAsync(options.Username, options.Password, ct);
        }
    }
}
