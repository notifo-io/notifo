// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.TestHelpers;

namespace Notifo.Domain.Channels.MobilePush;

public class MobilePushJobTests
{
    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var sut = new MobilePushJob(
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "My Subject"
                }
            },
            new ChannelSetting
            {
                Send = ChannelSend.Send
            },
            Guid.NewGuid(),
            new MobilePushToken
            {
                Token = "Token",
                DeviceIdentifier = "DeviceID",
                DeviceType = MobileDeviceType.iOS
            },
            true);

        var serialized = sut.SerializeAndDeserialize();

        serialized.Should().BeEquivalentTo(sut);
    }
}
