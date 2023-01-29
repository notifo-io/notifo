// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.Smtp;

public sealed partial class SmtpIntegration : IEmailSender
{
    private const int Attempts = 5;

    public async Task<DeliveryResult> SendAsync(IntegrationContext context, EmailMessage request,
        CancellationToken ct)
    {
        var fromEmail = FromEmailProperty.GetString(context.Properties);
        var fromName = FromNameProperty.GetString(context.Properties);

        if (string.IsNullOrWhiteSpace(request.FromEmail))
        {
            request = request with { FromEmail = fromEmail };
        }

        if (string.IsNullOrWhiteSpace(request.FromName))
        {
            request = request with { FromName = fromName };
        }

        var options = new SmtpOptions
        {
            Username = UsernameProperty.GetString(context.Properties),
            Host = HostProperty.GetString(context.Properties),
            HostPort = (int)HostPortProperty.GetNumber(context.Properties),
            Password = PasswordProperty.GetString(context.Properties)
        };

        // Try a few attempts to get a non-disposed server instance.
        for (var i = 1; i <= Attempts; i++)
        {
            try
            {
                var server = serverPool.GetServer(options);

                await server.SendAsync(request, ct);
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

        return DeliveryResult.Sent;
    }
}
