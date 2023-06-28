// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.TestHelpers;

namespace Notifo.Domain.Integrations.Telekom;

[Trait("Category", "Dependencies")]
public sealed class TelekomTests : SmsSenderTestBase
{
    protected override ResolvedIntegration<ISmsSender> CreateSender()
    {
        var apiKey = TestUtils.Configuration.GetValue<string>("sms:telekom:apiKey")!;
        var phoneNumber = TestUtils.Configuration.GetValue<string>("sms:telekom:phoneNumber")!;
        var phoneNumbers = TestUtils.Configuration.GetValue<string>("sms:telekom:phoneNumbers")!;

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
