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

public sealed class HandledInfoDto
{
    /// <summary>
    /// The timestamp.
    /// </summary>
    public Instant Timestamp { get; set; }

    /// <summary>
    /// The channel over which the notification was marked as seen or confirmed.
    /// </summary>
    public string? Channel { get; set; }

    public static HandledInfoDto FromDomainObject(HandledInfo source)
    {
        return SimpleMapper.Map(source, new HandledInfoDto());
    }
}
