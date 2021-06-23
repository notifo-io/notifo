// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Notifications.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.UserNotifications;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Notifications
{
    [OpenApiTag("Notifications")]
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
        /// <returns>
        /// 200 => User notifications returned.
        /// 404 => User not found.
        /// </returns>
        [HttpGet("api/apps/{appId}/users/{id}/notifications")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(ListResponseDto<UserNotificationDetailsDto>))]
        public async Task<IActionResult> GetNotifications(string appId, string id, [FromQuery] QueryDto q)
        {
            var notifications = await userNotificationsStore.QueryAsync(appId, id, q.ToQuery<UserNotificationQuery>(true), HttpContext.RequestAborted);

            var response = new ListResponseDto<UserNotificationDetailsDto>
            {
                Items = notifications.Select(UserNotificationDetailsDto.FromDomainObjectAsDetails).ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Query user notifications of the current user.
        /// </summary>
        /// <param name="q">The query object.</param>
        /// <returns>
        /// 200 => Notifications returned.
        /// </returns>
        [HttpGet("/api/me/notifications")]
        [AppPermission(NotifoRoles.AppUser)]
        [Produces(typeof(ListResponseDto<UserNotificationDto>))]
        public async Task<IActionResult> GetMyNotifications([FromQuery] QueryDto q)
        {
            var notifications = await userNotificationsStore.QueryAsync(App.Id, UserId, q.ToQuery<UserNotificationQuery>(false), HttpContext.RequestAborted);

            var response = new ListResponseDto<UserNotificationDto>
            {
                Items = notifications.Select(UserNotificationDto.FromDomainObject).ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Query archhived user notifications of the current user.
        /// </summary>
        /// <returns>
        /// 200 => Notifications returned.
        /// </returns>
        [HttpGet("/api/me/notifications/archive")]
        [AppPermission(NotifoRoles.AppUser)]
        [Produces(typeof(ListResponseDto<UserNotificationDto>))]
        public async Task<IActionResult> GetMyArchive()
        {
            var notifications = await userNotificationsStore.QueryAsync(App.Id, UserId, ArchiveQuery, HttpContext.RequestAborted);

            var response = new ListResponseDto<UserNotificationDto>
            {
                Items = notifications.Select(UserNotificationDto.FromDomainObject).ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Confirms the user notifications for the current user.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>
        /// 204 => Notifications updated.
        /// </returns>
        [HttpPost("/api/me/notifications/handled")]
        [AppPermission(NotifoRoles.AppUser)]
        public async Task<IActionResult> ConfirmMe([FromBody] TrackNotificationDto request)
        {
            var details = request.ToDetails();

            if (request.Confirmed.HasValue)
            {
                await userNotificationService.TrackConfirmedAsync(request.Confirmed.Value, details);
            }

            if (request.Seen?.Length > 0)
            {
                await userNotificationService.TrackSeenAsync(request.Seen, details);
            }

            return NoContent();
        }
    }
}
