// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations.Mailjet;

public sealed class MailjetEmailSender : IEmailSender
{
    private const int Attempts = 5;
    private readonly Func<MailjetEmailServer> server;
    private readonly string fromEmail;
    private readonly string fromName;

    public string Name => "Mailjet";

    public MailjetEmailSender(Func<MailjetEmailServer> server,
        string fromEmail,
        string fromName)
    {
        this.server = server;

        this.fromEmail = fromEmail;
        this.fromName = fromName;
    }

    public async Task SendAsync(EmailMessage request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FromEmail))
        {
            request = request with { FromEmail = fromEmail };
        }

        if (string.IsNullOrWhiteSpace(request.FromName))
        {
            request = request with { FromName = fromName };
        }

        // Try a few attempts to get a non-disposed server instance.
        for (var i = 1; i <= Attempts; i++)
        {
            try
            {
                await server().SendAsync(request);
                break;
            }
            catch (ObjectDisposedException)
            {
                if (i == Attempts)
                {
                    throw;
                }
            }
        }
    }
}
