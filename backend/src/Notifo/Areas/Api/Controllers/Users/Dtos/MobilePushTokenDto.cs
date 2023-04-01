// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Users.Dtos;

public sealed class MobilePushTokenDto
{
    /// <summary>
    /// The token.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// The device type.
    /// </summary>
    public MobileDeviceType DeviceType { get; set; }

    /// <summary>
    /// A unique identifier for the device.
    /// </summary>
    public string? DeviceIdentifier { get; set; }

    /// <summary>
    /// The last time the device was woken up.
    /// </summary>
    public Instant? LastWakeup { get; set; }

    public static MobilePushTokenDto FromDomainObject(MobilePushToken source)
    {
        var result = SimpleMapper.Map(source, new MobilePushTokenDto());

        if (source.LastWakeup != default)
        {
            result.LastWakeup = source.LastWakeup;
        }
        else
        {
            result.LastWakeup = null;
        }

        return result;
    }
}
