// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Apps;
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
            A.Fake<ISmsCallback>(),
            A.Fake<ISmsUrl>(),
            apiKey,
            phoneNumberFrom,
            "1");

        var app = new App("1", default);

        var response = await sut.SendAsync(app, phoneNumberTo, "Hello Telekom", "1");

        Assert.Equal(SmsResult.Sent, response);
    }
}
