// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;

namespace Notifo.Domain.Integrations.Mailchimp;

[Trait("Category", "Dependencies")]
public class MailchimpTests : EmailSenderTestBase
{
    protected override IEmailSender CreateSender()
    {
        var apiKey = TestHelpers.Configuration.GetValue<string>("mailchimp:apiKey")!;
        var fromEmail = TestHelpers.Configuration.GetValue<string>("mailchimp:fromEmail")!;
        var fromName = TestHelpers.Configuration.GetValue<string>("mailchimp:fromName")!;

        var clientFactory = A.Fake<IHttpClientFactory>();

        A.CallTo(() => clientFactory.CreateClient(A<string>._))
            .ReturnsLazily(() => new HttpClient());

        return new MailchimpEmailSender(clientFactory, apiKey, fromEmail, fromName);
    }
}
