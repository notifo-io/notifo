// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.Mailjet;

public sealed class MailjetEmailServer
{
    private readonly MailjetClient mailjetClient;

    public MailjetEmailServer(MailjetClient mailjetClient)
    {
        this.mailjetClient = mailjetClient;
    }

    public async Task SendAsync(EmailMessage message)
    {
        var email = new TransactionalEmailBuilder()
            .WithFrom(new SendContact(
                message.FromEmail,
                message.FromName))
            .WithTo(new SendContact(
                message.ToEmail,
                message.ToName))
            .WithSubject(message.Subject)
            .WithHtmlPart(message.BodyHtml)
            .WithTextPart(message.BodyText)
            .Build();

        var responses = await mailjetClient.SendTransactionalEmailAsync(email);

        if (responses.Messages is not { Length: 1 })
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.Mailjet_Error, message.FromEmail);

            throw new DomainException(errorMessage);
        }

        var response = responses.Messages[0];
        var responseError = response.Errors?.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(responseError?.ErrorMessage))
        {
            var errorMessage =
                string.Format(CultureInfo.CurrentCulture, Texts.Mailjet_Error,
                    message.FromEmail,
                    responseError.ErrorMessage,
                    responseError.ErrorCode);

            throw new DomainException(errorMessage);
        }
    }
}
