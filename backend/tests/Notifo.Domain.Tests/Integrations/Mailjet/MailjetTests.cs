// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Mailjet.Client;
using Microsoft.Extensions.Configuration;

namespace Notifo.Domain.Integrations.Mailjet;

[Trait("Category", "Dependencies")]
public class MailjetTests : EmailSenderTestBase
{
    protected override IEmailSender CreateSender()
    {
        var apiKey = TestHelpers.Configuration.GetValue<string>("mailjet:apiKey")!;
        var apiSecret = TestHelpers.Configuration.GetValue<string>("mailjet:apiSecret")!;
        var fromEmail = TestHelpers.Configuration.GetValue<string>("mailjet:fromEmail")!;
        var fromName = TestHelpers.Configuration.GetValue<string>("mailjet:fromName")!;

        var client = new MailjetClient(apiKey, apiSecret);

        return new MailjetEmailSender(() => new MailjetEmailServer(client), fromEmail, fromName);
    }
}
