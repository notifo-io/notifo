// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Integrations.Mailjet;

namespace Notifo.Domain.Integrations.Mailchimp;

[Trait("Category", "Dependencies")]
public class MailchimpTests : EmailSenderTestBase
{
    protected override ResolvedIntegration<IEmailSender> CreateSender()
    {
        var apiKey = TestHelpers.Configuration.GetValue<string>("email:mailchimp:apiKey")!;
        var fromEmail = TestHelpers.Configuration.GetValue<string>("email:mailchimp:fromEmail")!;
        var fromName = TestHelpers.Configuration.GetValue<string>("email:mailchimp:fromName")!;

        var context = BuildContext(new Dictionary<string, string>
        {
            [MailchimpIntegration.ApiKeyProperty.Name] = apiKey,
            [MailchimpIntegration.FromEmailProperty.Name] = fromEmail,
            [MailchimpIntegration.FromNameProperty.Name] = fromName,
        });

        var integration =
            new ServiceCollection()
                .AddIntegrationMailchimp()
                .AddMemoryCache()
                .AddHttpClient()
                .BuildServiceProvider()
                .GetRequiredService<MailjetIntegration>();

        return new ResolvedIntegration<IEmailSender>(Guid.NewGuid().ToString(), context, integration);
    }
}
