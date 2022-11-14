// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.WebPush.Dtos;
using Notifo.Domain.Identity;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.WebPush;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class WebPushController : BaseController
{
    [HttpPost("api/me/webpush")]
    [HttpPost("api/webpush")]
    [AppPermission(NotifoRoles.AppUser)]
    public async Task<IActionResult> PostMyToken([FromBody] RegisterWebTokenDto request)
    {
        var command = request.ToUpdate(UserId);

        await Mediator.Send(command, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpDelete("api/me/webpush")]
    [HttpDelete("api/webpush")]
    [AppPermission(NotifoRoles.AppUser)]
    public async Task<IActionResult> DeleteMyToken([FromBody] RegisterWebTokenDto request)
    {
        var command = request.ToDelete(UserId);

        await Mediator.Send(command, HttpContext.RequestAborted);

        return NoContent();
    }
}
