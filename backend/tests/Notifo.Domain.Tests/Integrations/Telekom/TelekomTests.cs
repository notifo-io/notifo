// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Integrations.Telekom;

[Trait("Category", "Dependencies")]
public sealed class TelekomTests : SmsSenderTestBase
{
    protected override ResolvedIntegration<ISmsSender> CreateSender()
    {
        var apiKey = TestHelpers.Configuration.GetValue<string>("sms:telekom:apiKey")!;
        var phoneNumber = TestHelpers.Configuration.GetValue<string>("sms:telekom:phoneNumber")!;
        var phoneNumbers = TestHelpers.Configuration.GetValue<string>("sms:telekom:phoneNumbers")!;

        var context = BuildContext(new Dictionary<string, string>
        {
            [TelekomSmsIntegration.ApiKeyProperty.Name] = apiKey,
            [TelekomSmsIntegration.PhoneNumberProperty.Name] = phoneNumber,
            [TelekomSmsIntegration.PhoneNumberProperty.Name] = phoneNumbers,
        });

        var integration =
            new ServiceCollection()
                .AddIntegrationTelekom()
                .AddMemoryCache()
                .AddHttpClient()
                .BuildServiceProvider()
                .GetRequiredService<TelekomSmsIntegration>();

        return new ResolvedIntegration<ISmsSender>(Guid.NewGuid().ToString(), context, integration);
    }
}
