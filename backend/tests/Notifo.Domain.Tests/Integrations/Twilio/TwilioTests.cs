// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Integrations.Twilio;

[Trait("Category", "Dependencies")]
public sealed class TwilioTests : SmsSenderTestBase
{
    protected override ResolvedIntegration<ISmsSender> CreateSender()
    {
        var authToken = TestHelpers.Configuration.GetValue<string>("sms:twilio:authToken")!;
        var accountSid = TestHelpers.Configuration.GetValue<string>("sms:twilio:accountSid")!;
        var phoneNumber = TestHelpers.Configuration.GetValue<string>("sms:twilio:phoneNumber")!;

        var context = BuildContext(new Dictionary<string, string>
        {
            [TwilioSmsIntegration.AuthTokenProperty.Name] = authToken,
            [TwilioSmsIntegration.AccountSidProperty.Name] = accountSid,
            [TwilioSmsIntegration.PhoneNumberProperty.Name] = phoneNumber,
        });

        var integration =
            new ServiceCollection()
                .AddIntegrationTwilio()
                .AddMemoryCache()
                .AddHttpClient()
                .BuildServiceProvider()
                .GetRequiredService<TwilioSmsIntegration>();

        return new ResolvedIntegration<ISmsSender>(Guid.NewGuid().ToString(), context, integration);
    }
}
