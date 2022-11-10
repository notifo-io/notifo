// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Notifications.Dtos;
using Notifo.Domain;
using Notifo.Domain.Identity;
using Notifo.Domain.UserNotifications;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Notifications;

[ApiExplorerSettings(GroupName = "Notifications")]
public sealed class NotificationsController : BaseController
{
    private static readonly UserNotificationQuery ArchiveQuery = new UserNotificationQuery { Take = 100, Scope = UserNotificationQueryScope.Deleted };
    private readonly IUserNotificationStore userNotificationsStore;
    private readonly IUserNotificationService userNotificationService;

    public NotificationsController(
        IUserNotificationStore userNotificationsStore,
        IUserNotificationService userNotificationService)
    {
        this.userNotificationsStore = userNotificationsStore;
        this.userNotificationService = userNotificationService;
    }

    /// <summary>
    /// Query user notifications.
    /// </summary>
    /// <param name="appId">The app where the user belongs to.</param>
    /// <param name="id">The user id.</param>
    /// <param name="q">The query object.</param>
    /// <response code="200">User notifications returned.</response>.
    /// <response code="404">User or app not found.</response>.
    [HttpGet("api/apps/{appId:notEmpty}/users/{id:notEmpty}/notifications")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [Produces(typeof(ListResponseDto<UserNotificationDetailsDto>))]
    public async Task<IActionResult> GetNotifications(string appId, string id, [FromQuery] UserNotificationQueryDto q)
    {
        var notifications = await userNotificationsStore.QueryAsync(appId, id, q.ToQuery(true), HttpContext.RequestAborted);

        var response = new ListResponseDto<UserNotificationDetailsDto>();

        response.Items.AddRange(notifications.Select(UserNotificationDetailsDto.FromDomainObjectAsDetails));
        response.Total = notifications.Total;

        return Ok(response);
    }

    /// <summary>
    /// Query user notifications of the current user.
    /// </summary>
    /// <param name="q">The query object.</param>
    /// <response code="200">Notifications returned.</response>.
    [HttpGet]
    [Route("api/me/notifications")]
    [AppPermission(NotifoRoles.AppUser)]
    [Produces(typeof(ListResponseDto<UserNotificationDto>))]
    public async Task<IActionResult> GetMyNotifications([FromQuery] UserNotificationQueryDto q)
    {
        var notifications = await userNotificationsStore.QueryAsync(App.Id, UserId, q.ToQuery(true), HttpContext.RequestAborted);

        var response = new ListResponseDto<UserNotificationDto>();

        response.Items.AddRange(notifications.Select(UserNotificationDto.FromDomainObject));
        response.Total = notifications.Total;

        return Ok(response);
    }

    /// <summary>
    /// Query archhived user notifications of the current user.
    /// </summary>
    /// <response code="200">Notifications returned.</response>.
    [HttpGet]
    [Route("api/me/notifications/archive")]
    [AppPermission(NotifoRoles.AppUser)]
    [Produces(typeof(ListResponseDto<UserNotificationDto>))]
    public async Task<IActionResult> GetMyArchive()
    {
        var notifications = await userNotificationsStore.QueryAsync(App.Id, UserId, ArchiveQuery, HttpContext.RequestAborted);

        var response = new ListResponseDto<UserNotificationDto>();

        response.Items.AddRange(notifications.Select(UserNotificationDto.FromDomainObject));
        response.Total = notifications.Total;

        return Ok(response);
    }

    /// <summary>
    /// Confirms the user notifications for the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <response code="204">Notifications updated.</response>.
    [HttpPost("api/me/notifications/handled")]
    [AppPermission(NotifoRoles.AppUser)]
    public async Task<IActionResult> ConfirmMe([FromBody] TrackNotificationDto request)
    {
        if (request.Confirmed != null)
        {
            var token = TrackingToken.Parse(request.Confirmed, request.Channel, request.ConfigurationId);

            await userNotificationService.TrackConfirmedAsync(token);
        }

        if (request.Seen?.Length > 0)
        {
            var tokens = request.Seen.Select(x => TrackingToken.Parse(x, request.Channel, request.ConfigurationId));

            await userNotificationService.TrackSeenAsync(tokens);
        }

        return NoContent();
    }
}
