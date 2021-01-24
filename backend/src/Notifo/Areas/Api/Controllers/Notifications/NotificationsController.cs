// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Notifications.Dto;
using Notifo.Domain.Identity;
using Notifo.Domain.UserNotifications;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Notifications
{
    [OpenApiIgnore]
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

        [HttpGet("/api/me/notifications/archive")]
        [AppPermission(NotifoRoles.AppUser)]
        public async Task<IActionResult> GetArchive()
        {
            var notifications = await userNotificationsStore.QueryAsync(App.Id, UserId, ArchiveQuery, HttpContext.RequestAborted);

            var response = new ListResponseDto<NotificationDto>
            {
                Items = notifications.Select(NotificationDto.FromNotification).ToList()
            };

            return Ok(response);
        }

        [HttpPost("/api/me/notifications/handled")]
        [AppPermission(NotifoRoles.AppUser)]
        public async Task Confirm([FromBody] TrackNotificationDto request)
        {
            if (request.Confirmed.HasValue)
            {
                await userNotificationService.TrackConfirmedAsync(request.Confirmed.Value, request.Channel);
            }

            if (request.Seen?.Length > 0)
            {
                await userNotificationService.TrackSeenAsync(request.Seen, request.Channel);
            }
        }
    }
}
