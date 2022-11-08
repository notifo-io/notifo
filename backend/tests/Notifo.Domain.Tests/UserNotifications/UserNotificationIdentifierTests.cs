// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.TestHelpers;
using Xunit;

namespace Notifo.Domain.UserNotifications
{
    public class UserNotificationIdentifierTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var sut = new UserNotificationIdentifier
            {
                AppId = "app",
                Channel = "channel",
                ConfigurationId = Guid.NewGuid(),
                EventId = "event",
                Topic = "topic",
                UserId = "user",
                UserNotificationId = Guid.NewGuid()
            };

            var serialized = sut.SerializeAndDeserialize();

            Assert.Equal(sut, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_bson()
        {
            var sut = new UserNotificationIdentifier
            {
                AppId = "app",
                Channel = "channel",
                ConfigurationId = Guid.NewGuid(),
                EventId = "event",
                Topic = "topic",
                UserId = "user",
                UserNotificationId = Guid.NewGuid()
            };

            var serialized = sut.SerializeAndDeserializeBson();

            Assert.Equal(sut, serialized);
        }
    }
}
