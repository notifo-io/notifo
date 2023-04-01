// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Integrations.Smtp;
using Notifo.Infrastructure.KeyValueStore;

namespace Notifo.Domain.Integrations.AmazonSES;

[Trait("Category", "Dependencies")]
public class AmazonSESEmailServerTests : EmailSenderTestBase
{
    protected override ResolvedIntegration<IEmailSender> CreateSender()
    {
        var options = TestHelpers.Configuration.GetSection("email:amazonSES").Get<AmazonSESOptions>() ?? new AmazonSESOptions();

        var context = BuildContext(new Dictionary<string, string>
        {
            [SmtpIntegration.HostProperty.Name] = options.HostName,
            [SmtpIntegration.HostPortProperty.Name] = options.HostPort.ToString(CultureInfo.InvariantCulture),
            [SmtpIntegration.UsernameProperty.Name] = options.Username,
            [SmtpIntegration.PasswordProperty.Name] = options.Password,
        });

        var integration =
            new ServiceCollection()
                .AddIntegrationAmazonSES(A.Fake<IConfiguration>())
                .AddMemoryCache()
                .AddSingleton(A.Fake<IKeyValueStore>())
                .BuildServiceProvider()
                .GetRequiredService<IntegratedAmazonSESIntegration>();

        return new ResolvedIntegration<IEmailSender>(Guid.NewGuid().ToString(), context, integration);
    }
}
