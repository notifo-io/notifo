// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;
using Notifo.Domain.Utils;

namespace Notifo.Domain.Channels;

public class ChannelExtensionsTests
{
    private readonly IImageFormatter imageFormatter = A.Fake<IImageFormatter>();
    private readonly UserNotification notification = new UserNotification
    {
        Formatting = new NotificationFormatting<string>
        {
            Subject = "Subject",
            ImageSmall = "image/small",
            ImageLarge = "image/large"
        }
    };

    public ChannelExtensionsTests()
    {
        A.CallTo(() => imageFormatter.AddPreset(A<string?>._, A<string>._))
            .ReturnsLazily(x => x.GetArgument<string?>(0));
    }

    [Fact]
    public void Should_format_small_image()
    {
        Assert.Equal("image/small", notification.ImageSmall(imageFormatter, "small"));
    }

    [Fact]
    public void Should_format_large_image()
    {
        Assert.Equal("image/large", notification.ImageLarge(imageFormatter, "large"));
    }
}
