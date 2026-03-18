// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Integrations.Seven;

[Trait("Category", "Dependencies")]
public class SevenTests : SmsSenderTestBase
{
    protected override ResolvedIntegration<ISmsSender> CreateSender()
    {
        var apiKey = TestHelpers.Configuration.GetValue<string>("sms:seven:apiKey")!;
        var from = TestHelpers.Configuration.GetValue<string>("sms:seven:from");

        var context = BuildContext(new Dictionary<string, string>
        {
            [SevenSmsIntegration.ApiKeyProperty.Name] = apiKey,
            [SevenSmsIntegration.FromProperty.Name] = from ?? string.Empty,
        });

        var integration =
            new ServiceCollection()
                .AddIntegrationSeven()
                .AddMemoryCache()
                .AddHttpClient()
                .BuildServiceProvider()
                .GetRequiredService<SevenSmsIntegration>();

        return new ResolvedIntegration<ISmsSender>(Guid.NewGuid().ToString(), context, integration);
    }
}
