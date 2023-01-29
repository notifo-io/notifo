// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Integrations.Smtp;

[Trait("Category", "Dependencies")]
public class SmtpEmailServerTests : EmailSenderTestBase
{
    protected override ResolvedIntegration<IEmailSender> CreateSender()
    {
        var options = TestHelpers.Configuration.GetSection("email:smtp").Get<SmtpOptions>() ?? new SmtpOptions();

        var context = BuildContext(new Dictionary<string, string>
        {
            [SmtpIntegration.HostPortProperty.Name] = options.HostPort.ToString(CultureInfo.InvariantCulture),
            [SmtpIntegration.HostProperty.Name] = options.Host,
            [SmtpIntegration.PasswordProperty.Name] = options.Password,
            [SmtpIntegration.UsernameProperty.Name] = options.Username,
        });

        var integration =
            new ServiceCollection()
                .AddIntegrationSmtp()
                .AddMemoryCache()
                .BuildServiceProvider()
                .GetRequiredService<SmtpIntegration>();

        return new ResolvedIntegration<IEmailSender>(Guid.NewGuid().ToString(), context, integration);
    }
}
