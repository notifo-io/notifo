// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.TestHelpers;

namespace Notifo.Domain.Integrations.Mailjet;

[Trait("Category", "Dependencies")]
public class MailjetTests : EmailSenderTestBase
{
    protected override ResolvedIntegration<IEmailSender> CreateSender()
    {
        var apiKey = TestUtils.Configuration.GetValue<string>("email:mailjet:apiKey")!;
        var apiSecret = TestUtils.Configuration.GetValue<string>("email:mailjet:apiSecret")!;
        var fromEmail = TestUtils.Configuration.GetValue<string>("email:mailjet:fromEmail")!;
        var fromName = TestUtils.Configuration.GetValue<string>("email:mailjet:fromName")!;

        var context = BuildContext(new Dictionary<string, string>
        {
            [MailjetIntegration.FromNameProperty.Name] = fromName,
            [MailjetIntegration.FromEmailProperty.Name] = fromEmail,
            [MailjetIntegration.ApiKeyProperty.Name] = apiKey,
            [MailjetIntegration.ApiSecretProperty.Name] = apiSecret,
        });

        var integration =
            new ServiceCollection()
                .AddIntegrationMailjet()
                .AddMemoryCache()
                .BuildServiceProvider()
                .GetRequiredService<MailjetIntegration>();

        return new ResolvedIntegration<IEmailSender>(Guid.NewGuid().ToString(), context, integration);
    }
}
