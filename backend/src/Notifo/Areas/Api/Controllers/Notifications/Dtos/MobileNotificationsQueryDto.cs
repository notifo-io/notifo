// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Notifications.Dtos;

public sealed class MobileNotificationsQueryDto
{
    /// <summary>
    /// The device identifier (aka mobile push token).
    /// </summary>
    public string? DeviceIdentifier { get; set; }

    /// <summary>
    /// The max age of the notifications.
    /// </summary>
    public Instant After { get; set; }

    /// <summary>
    /// The number of notifications to query.
    /// </summary>
    public int Take { get; set; }

    public MobileNotificationsQuery ToQuery()
    {
        return SimpleMapper.Map(this, new MobileNotificationsQuery());
    }
}
