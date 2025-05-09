﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Logs.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.Log;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Logs;

[ApiExplorerSettings(GroupName = "Logs")]
public class LogsController(ILogStore logStore) : BaseController
{
    /// <summary>
    /// Query log entries.
    /// </summary>
    /// <param name="appId">The app where the log entries belongs to.</param>
    /// <param name="q">The query object.</param>
    /// <response code="200">Log entries returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet("api/apps/{appId:notEmpty}/logs/")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(ListResponseDto<LogEntryDto>))]
    public async Task<IActionResult> GetLogs(string appId, [FromQuery] LogQueryDto q)
    {
        var medias = await logStore.QueryAsync(appId, q.ToQuery(true), HttpContext.RequestAborted);

        var response = new ListResponseDto<LogEntryDto>();

        response.Items.AddRange(medias.Select(LogEntryDto.FromDomainObject));
        response.Total = medias.Total;

        return Ok(response);
    }
}
