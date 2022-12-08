// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Log;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Logs.Dtos;

public sealed class LogQueryDto : QueryDto
{
    /// <summary>
    /// The systems.
    /// </summary>
    public string[]? Systems { get; set; }

    /// <summary>
    /// The user id.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// The event code.
    /// </summary>
    public int EventCode { get; set; }

    public LogQuery ToQuery(bool needsTotal)
    {
        var result = SimpleMapper.Map(this, new LogQuery
        {
            TotalNeeded = needsTotal
        });

        return result;
    }
}
