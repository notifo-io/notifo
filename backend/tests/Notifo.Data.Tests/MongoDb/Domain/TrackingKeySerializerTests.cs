// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain;
using Notifo.MongoDb.TestHelpers;

namespace Notifo.MongoDb.Domain;

public class TrackingKeySerializerTests
{
    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var sut = new TrackingKey
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
