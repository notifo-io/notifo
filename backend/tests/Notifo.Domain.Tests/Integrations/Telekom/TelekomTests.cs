// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Channels.Sms;

namespace Notifo.Domain.Integrations.Telekom;

[Trait("Category", "Dependencies")]
public sealed class TelekomTests
{
    private readonly string apiKey = TestHelpers.Configuration.GetValue<string>("telekom:apiKey")!;
    private readonly string phoneNumberFrom = TestHelpers.Configuration.GetValue<string>("telekom:phoneNumberFrom")!;
    private readonly string phoneNumberTo = TestHelpers.Configuration.GetValue<string>("telekom:phoneNumberTo")!;

    [Fact]
    public async Task Should_send_sms()
    {
        var clientFactory = A.Fake<IHttpClientFactory>();

        A.CallTo(() => clientFactory.CreateClient(A<string>._))
            .ReturnsLazily(() => new HttpClient());

        var sut = new TelekomSmsSender(clientFactory,
            apiKey,
            phoneNumberFrom);

        var request = new SmsRequest(phoneNumberTo, "Hello Telekom");
        var response = await sut.SendAsync(request);

        Assert.Equal(SmsResult.Sent, response);
    }
}
