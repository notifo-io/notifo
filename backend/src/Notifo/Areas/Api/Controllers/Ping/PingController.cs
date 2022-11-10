// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;

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
    /// <returns>
    /// 204 => Service ping successful.
    /// </returns>
    /// <remarks>
    /// Can be used to test, if the Squidex API is alive and responding.
    /// </remarks>
    [HttpGet]
    [Route("ping/")]
    public IActionResult GetPing()
    {
        return NoContent();
    }
}
