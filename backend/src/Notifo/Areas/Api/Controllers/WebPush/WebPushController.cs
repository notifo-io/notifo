// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.WebPush.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.Users;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.WebPush;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class WebPushController : BaseController
{
    private readonly IUserStore userStore;

    public WebPushController(IUserStore userStore)
    {
        this.userStore = userStore;
    }

    [HttpPost("api/me/webpush")]
    [HttpPost("api/webpush")]
    [AppPermission(NotifoRoles.AppUser)]
    public async Task<IActionResult> PostMyToken([FromBody] RegisterWebTokenDto request)
    {
        var command = new AddUserWebPushSubscription
        {
            Subscription = request.Subscription.ToSubscription()
        };

        await userStore.UpsertAsync(App.Id, UserId, command, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpDelete("api/me/webpush")]
    [HttpDelete("api/webpush")]
    [AppPermission(NotifoRoles.AppUser)]
    public async Task<IActionResult> DeleteMyToken([FromBody] RegisterWebTokenDto request)
    {
        var command = new RemoveUserWebPushSubscription
        {
            Endpoint = request.Subscription.Endpoint
        };

        await userStore.UpsertAsync(App.Id, UserId, command, HttpContext.RequestAborted);

        return NoContent();
    }
}
