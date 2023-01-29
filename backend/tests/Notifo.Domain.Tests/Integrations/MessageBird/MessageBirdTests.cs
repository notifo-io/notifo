// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Integrations.MessageBird;

[Trait("Category", "Dependencies")]
public class MessageBirdTests : SmsSenderTestBase
{
    protected override ResolvedIntegration<ISmsSender> CreateSender()
    {
        var accessKey = TestHelpers.Configuration.GetValue<string>("sms:messageBird:accessKey")!;
        var phoneNumber = TestHelpers.Configuration.GetValue<string>("sms:messageBird:phoneNumber")!;
        var phoneNumbers = TestHelpers.Configuration.GetValue<string>("sms:messageBird:phoneNumbers")!;

        var context = BuildContext(new Dictionary<string, string>
        {
            [MessageBirdSmsIntegration.AccessKeyProperty.Name] = accessKey,
            [MessageBirdSmsIntegration.PhoneNumberProperty.Name] = phoneNumber,
            [MessageBirdSmsIntegration.PhoneNumbersProperty.Name] = phoneNumbers,
        });

        var integration =
            new ServiceCollection()
                .AddIntegrationMessageBird(A.Fake<IConfiguration>())
                .AddMemoryCache()
                .AddHttpClient()
                .BuildServiceProvider()
                .GetRequiredService<MessageBirdSmsIntegration>();

        return new ResolvedIntegration<ISmsSender>(Guid.NewGuid().ToString(), context, integration);
    }
}
