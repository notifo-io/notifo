// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Twilio.Clients;

namespace Notifo.Domain.Integrations.Twilio;

[Trait("Category", "Dependencies")]
public sealed class TwilioTests
{
    private readonly string phoneNumberTo = TestHelpers.Configuration.GetValue<string>("twilio:phoneNumberTo")!;
    private readonly ISmsSender sut;

    public TwilioTests()
    {
        var apiUsername = TestHelpers.Configuration.GetValue<string>("twilio:apiUsername")!;
        var apiPassword = TestHelpers.Configuration.GetValue<string>("twilio:apiPassword")!;
        var phoneNumberFrom = TestHelpers.Configuration.GetValue<string>("twilio:phoneNumberFrom")!;

        var client = new TwilioRestClient(apiUsername, apiPassword);

        sut = new TwilioSmsSender(
            A.Fake<ISmsCallback>(),
            client,
            phoneNumberFrom);
    }

    [Fact]
    public async Task Should_send_sms()
    {
        var message = new SmsMessage(Guid.NewGuid(), phoneNumberTo, "Hello Twilio", null);

        var response = await sut.SendAsync(message, default);

        Assert.Equal(SmsResult.Sent, response);
    }
}
