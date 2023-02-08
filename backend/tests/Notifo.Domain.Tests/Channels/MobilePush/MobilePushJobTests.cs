// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.TestHelpers;

namespace Notifo.Domain.Channels.MobilePush;

public class MobilePushJobTests
{
    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var context = new ChannelContext
        {
            App = null!,
            AppId = null!,
            Configuration = new SendConfiguration(),
            ConfigurationId = Guid.NewGuid(),
            IsUpdate = false,
            Setting = new ChannelSetting(),
            User = null!,
            UserId = null!,
        };

        var sut = new MobilePushJob(
            new UserNotification
            {
                Formatting = new NotificationFormatting<string>
                {
                    Subject = "My Subject"
                }
            },
            context,
            new MobilePushToken
            {
                Token = "Token",
                DeviceIdentifier = "DeviceID",
                DeviceType = MobileDeviceType.iOS
            });

        var serialized = sut.SerializeAndDeserialize();

        serialized.Should().BeEquivalentTo(sut);
    }
}
