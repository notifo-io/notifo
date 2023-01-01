// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Notifo.Domain.Integrations.MessageBird.Implementation;

namespace Notifo.Domain.Integrations.MessageBird;

[Trait("Category", "Dependencies")]
public class MessageBirdTests
{
    private readonly string phoneNumberFrom = TestHelpers.Configuration.GetValue<string>("messageBird:phoneNumberFrom")!;
    private readonly string phoneNumberTo = TestHelpers.Configuration.GetValue<string>("messageBird:phoneNumberTo")!;
    private readonly ISmsSender sut;

    public MessageBirdTests()
    {
        var clientFactory = A.Fake<IHttpClientFactory>();

        A.CallTo(() => clientFactory.CreateClient(A<string>._))
            .ReturnsLazily(() => new HttpClient());

        var client = new MessageBirdClient(clientFactory, Options.Create(new MessageBirdOptions
        {
            PhoneNumber = phoneNumberFrom,
            PhoneNumbers = null,
            AccessKey = TestHelpers.Configuration.GetValue<string>("messageBird:accessKey")!
        }));
    }

    [Fact]
    public async Task Should_send_sms()
    {
        var message = new SmsMessage(Guid.NewGuid(), phoneNumberTo, "Hello Telekom", null);

        var response = await sut.SendAsync(message, default);

        Assert.Equal(SmsResult.Sent, response);
    }
}
