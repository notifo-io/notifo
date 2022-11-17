// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.UserNotifications;

public sealed class MobileNotificationsQuery
{
    public string? DeviceIdentifier { get; set; }

    public Instant After { get; set; }

    public int Take { get; set; }

    public UserNotificationQuery ToBaseQuery()
    {
        return SimpleMapper.Map(this, new UserNotificationQuery());
    }
}
