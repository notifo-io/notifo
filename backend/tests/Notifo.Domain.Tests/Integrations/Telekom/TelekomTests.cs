// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;

namespace Notifo.Domain.Integrations.Telekom;

[Trait("Category", "Dependencies")]
public sealed class TelekomTests
{
    private readonly string apiKey = TestHelpers.Configuration.GetValue<string>("telekom:apiKey")!;
    private readonly string phoneNumberFrom = TestHelpers.Configuration.GetValue<string>("telekom:phoneNumberFrom")!;
    private readonly string phoneNumberTo = TestHelpers.Configuration.GetValue<string>("telekom:phoneNumberTo")!;
    private readonly ISmsSender sut;

    public TelekomTests()
    {
        var clientFactory = A.Fake<IHttpClientFactory>();

        A.CallTo(() => clientFactory.CreateClient(A<string>._))
            .ReturnsLazily(() => new HttpClient());

        sut = new TelekomSmsSender(
            A.Fake<ISmsCallback>(),
            clientFactory,
            apiKey,
            phoneNumberFrom);
    }

    [Fact]
    public async Task Should_send_sms()
    {
        var message = new SmsMessage(Guid.NewGuid(), phoneNumberTo, "Hello Telekom", null);

        var response = await sut.SendAsync(message, default);

        Assert.Equal(SmsResult.Sent, response);
    }
}
