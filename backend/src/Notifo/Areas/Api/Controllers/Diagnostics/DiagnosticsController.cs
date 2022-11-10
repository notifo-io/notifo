// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Domain.Identity;
using Notifo.Infrastructure.Diagnostics;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Diagnostics;

/// <summary>
/// Makes a diagnostics request.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Diagnostics))]
public sealed class DiagnosticsController : BaseController
{
    private readonly Diagnoser dumper;

    public DiagnosticsController(Diagnoser dumper)
    {
        this.dumper = dumper;
    }

    /// <summary>
    /// Creates a dump and writes it into storage..
    /// </summary>
    /// <response code="204">Dump created successful.</response>.
    /// <response code="501">Not configured.</response>.
    [HttpGet]
    [Route("api/diagnostics/dump")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetDump()
    {
        var success = await dumper.CreateDumpAsync(HttpContext.RequestAborted);

        if (!success)
        {
            return StatusCode(501);
        }

        return NoContent();
    }

    /// <summary>
    /// Creates a gc dump and writes it into storage.
    /// </summary>
    /// <response code="204">Dump created successful.</response>.
    /// <response code="501">Not configured.</response>.
    [HttpGet]
    [Route("api/diagnostics/gcdump")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetGCDump()
    {
        var success = await dumper.CreateGCDumpAsync(HttpContext.RequestAborted);

        if (!success)
        {
            return StatusCode(501);
        }

        return NoContent();
    }
}
