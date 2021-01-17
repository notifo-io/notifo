// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Notifo.Areas.Api.Controllers.Notifications.Dto;
using Notifo.Domain;
using Notifo.Domain.UserNotifications;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Notifications
{
    [OpenApiIgnore]
    public sealed class NotificationsController : BaseController
    {
        private readonly IUserNotificationStore userNotificationsStore;
        private readonly IUserNotificationService userNotificationService;

        public NotificationsController(
            IUserNotificationStore userNotificationsStore,
            IUserNotificationService userNotificationService)
        {
            this.userNotificationsStore = userNotificationsStore;
            this.userNotificationService = userNotificationService;
        }

        [HttpGet("/api/notifications")]
        [AppPermission(Roles.User)]
        public async Task<IActionResult> GetNotifications([FromQuery] long etag = 0)
        {
            var query = new UserNotificationQuery
            {
                After = Instant.FromUnixTimeMilliseconds(etag),
                Take = 100
            };

            var notifications = await userNotificationsStore.QueryAsync(App.Id, UserId, query, HttpContext.RequestAborted);

            var response = new NotificationsDto
            {
                Notifications = notifications.Select(NotificationDto.FromNotification).ToArray()
            };

            if (notifications.Any())
            {
                response.Etag = notifications.Max(x => x.Updated).ToUnixTimeMilliseconds();
            }
            else if (query.After != default)
            {
                return NoContent();
            }

            return Ok(response);
        }

        [HttpPost("/api/notifications/handled")]
        [AppPermission(Roles.User)]
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
