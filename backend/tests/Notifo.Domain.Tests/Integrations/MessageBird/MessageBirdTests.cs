﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Xunit;

namespace Notifo.Domain.Integrations.MessageBird
{
    [Trait("Category", "Dependencies")]
    public class MessageBirdTests
    {
        private readonly MessageBirdClient sut;

        public MessageBirdTests()
        {
            var clientFactory = A.Fake<IHttpClientFactory>();

            A.CallTo(() => clientFactory.CreateClient(A<string>._))
                .ReturnsLazily(() => new HttpClient());

            sut = new MessageBirdClient(clientFactory, Options.Create(new MessageBirdOptions
            {
                PhoneNumber = TestHelpers.Configuration.GetValue<string>("messageBird:phoneNumber"),
                PhoneNumbers = null,
                AccessKey = TestHelpers.Configuration.GetValue<string>("messageBird:accessKey")
            }));
        }

        [Fact]
        public async Task Should_send_sms()
        {
            var sms = new MessageBirdSmsMessage("4917683297281", "Hello");

            var response = await sut.SendSmsAsync(sms, default);

            Assert.Equal(1, response.Recipients.TotalSentCount);
        }
    }
}
