// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Areas.Api.OpenApi;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Events.Dtos;

[OpenApiRequest]
public class CancelEventDto
{
    /// <summary>
    /// The user ID for which the event was created.
    /// </summary>
    [Required]
    public string UserId { get; set; }

    /// <summary>
    /// The event ID.
    /// </summary>
    [Required]
    public string EventId { get; set; }

    /// <summary>
    /// The grouping key to combine notifications.
    /// </summary>
    public string? GroupKey { get; set; }

    /// <summary>
    /// True when using test integrations.
    /// </summary>
    public bool Test { get; set; }

    public CancelRequest ToRequest(string appId)
    {
        var result = SimpleMapper.Map(this, new CancelRequest());

        result.AppId = appId;

        return result;
    }
}
