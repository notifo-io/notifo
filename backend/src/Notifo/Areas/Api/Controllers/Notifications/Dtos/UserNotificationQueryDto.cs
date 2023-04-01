// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Areas.Api.OpenApi;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Notifications.Dtos;

[OpenApiRequest]
public sealed class UserNotificationQueryDto : QueryDto
{
    /// <summary>
    /// The active channels.
    /// </summary>
    public string[]? Channels { get; set; }

    /// <summary>
    /// The source channel.
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// The correlation ID, that can be used to query notifications.
    /// </summary>
    public string? CorrelationId { get; set; }

    public UserNotificationQuery ToQuery(bool needsTotal)
    {
        var result = SimpleMapper.Map(this, new UserNotificationQuery
        {
            TotalNeeded = needsTotal
        });

        return result;
    }
}
