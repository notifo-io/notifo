// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Domain.Integrations;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.MobilePush.Dtos;

public sealed class RegisterMobileTokenDto
{
    /// <summary>
    /// The device token.
    /// </summary>
    [Required]
    public string Token { get; set; }

    /// <summary>
    /// The device type.
    /// </summary>
    public MobileDeviceType DeviceType { get; set; }

    /// <summary>
    /// A unique identifier for the device.
    /// </summary>
    public string? DeviceIdentifier { get; set; }

    public MobilePushToken ToToken()
    {
        return SimpleMapper.Map(this, new MobilePushToken());
    }

    public AddUserMobileToken ToUpdate(string userId)
    {
        var result = new AddUserMobileToken
        {
            Token = ToToken(),

            // User ID is coming from the route in this context.
            UserId = userId
        };

        return result;
    }
}
