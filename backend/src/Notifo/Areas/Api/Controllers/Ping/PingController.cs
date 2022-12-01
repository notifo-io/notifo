// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Ping.Dtos;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Ping;

/// <summary>
/// Makes a ping request.
/// </summary>
[ApiExplorerSettings(GroupName = "Ping")]
public sealed class PingController : BaseController
{
    /// <summary>
    /// Get ping status of the API.
    /// </summary>
    /// <response code="204">Service ping successful.</response>.
    /// <remarks>
    /// Can be used to test, if the Squidex API is alive and responding.
    /// </remarks>
    [HttpGet]
    [Route("ping/")]
    [Obsolete("Use /api/ping instead.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetOldPing()
    {
        return NoContent();
    }

    /// <summary>
    /// Get ping status of the API.
    /// </summary>
    /// <response code="204">Service ping successful.</response>.
    /// <remarks>
    /// Can be used to test, if the Squidex API is alive and responding.
    /// </remarks>
    [HttpGet]
    [Route("api/ping/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetPing()
    {
        return NoContent();
    }

    /// <summary>
    /// Get some info about the API.
    /// </summary>
    /// <response code="204">Service info returned.</response>.
    /// <remarks>
    /// Can be used to test, if the Squidex API is alive and responding.
    /// </remarks>
    [HttpGet]
    [Route("api/info/")]
    [Produces(typeof(InfoDto))]
    public IActionResult GetInfo()
    {
        var response = new InfoDto { Version = VersionProvider.Current };

        return Ok(response);
    }
}
