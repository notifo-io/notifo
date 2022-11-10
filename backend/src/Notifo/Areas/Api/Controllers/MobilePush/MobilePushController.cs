// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.MobilePush.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.Users;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.MobilePush;

[ApiExplorerSettings(GroupName = "MobilePush")]
public sealed class MobilePushController : BaseController
{
    private readonly IUserStore userStore;

    public MobilePushController(IUserStore userStore)
    {
        this.userStore = userStore;
    }

    /// <summary>
    /// Returns the mobile push tokens.
    /// </summary>
    /// <response code="200">Mobile push tokens returned.</response>.
    [HttpGet("api/me/mobilepush")]
    [AppPermission(NotifoRoles.AppUser)]
    [Produces(typeof(ListResponseDto<MobilePushTokenDto>))]
    public async Task<IActionResult> GetMyToken()
    {
        var user = await userStore.GetAsync(App.Id, UserId, HttpContext.RequestAborted);

        if (user == null)
        {
            return NotFound();
        }

        var response = new ListResponseDto<MobilePushTokenDto>();

        response.Items.AddRange(user.MobilePushTokens.Select(MobilePushTokenDto.FromDomainObject));
        response.Total = user.MobilePushTokens.Count;

        return Ok(response);
    }

    /// <summary>
    /// Register a mobile push token for the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <response code="204">Mobile push token registered.</response>.
    [HttpPost("api/mobilepush")]
    [AppPermission(NotifoRoles.AppUser)]
    [Obsolete("Use new endpoint <api/me/mobilepush>")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> PostMyTokenOld([FromBody] RegisterMobileTokenDto request)
    {
        return PostMyToken(request);
    }

    /// <summary>
    /// Register a mobile push token for the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <response code="204">Mobile push token registered.</response>.
    [HttpPost("api/me/mobilepush")]
    [AppPermission(NotifoRoles.AppUser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PostMyToken([FromBody] RegisterMobileTokenDto request)
    {
        var command = new AddUserMobileToken
        {
            Token = request.ToToken()
        };

        await userStore.UpsertAsync(App.Id, UserId, command, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Deletes a mobile push token for the current user.
    /// </summary>
    /// <param name="token">The token to remove.</param>
    /// <response code="204">Mobile push token removed.</response>.
    [HttpDelete("api/mobilepush/{token:notEmpty}")]
    [AppPermission(NotifoRoles.AppUser)]
    [Obsolete("Use new endpoint <api/me/mobilepush/{token:notEmpty}>")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<IActionResult> DeleteMyTokenOld(string token)
    {
        return DeleteMyToken(token);
    }

    /// <summary>
    /// Deletes a mobile push token for the current user.
    /// </summary>
    /// <param name="token">The token to remove.</param>
    /// <response code="204">Mobile push token removed.</response>.
    [HttpDelete("api/me/mobilepush/{token:notEmpty}")]
    [AppPermission(NotifoRoles.AppUser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMyToken(string token)
    {
        var command = new RemoveUserMobileToken
        {
            Token = token
        };

        await userStore.UpsertAsync(App.Id, UserId, command, HttpContext.RequestAborted);

        return NoContent();
    }
}
