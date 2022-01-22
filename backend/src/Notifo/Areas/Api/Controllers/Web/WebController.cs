// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NodaTime;
using Notifo.Areas.Api.Controllers.Notifications.Dtos;
using Notifo.Areas.Api.Controllers.Web.Dtos;
using Notifo.Domain.Channels;
using Notifo.Domain.Identity;
using Notifo.Domain.UserNotifications;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Web
{
    [OpenApiIgnore]
    public sealed class WebController : BaseController
    {
        private readonly IUserNotificationStore userNotificationStore;
        private readonly SignalROptions signalROptions;

        public WebController(IUserNotificationStore userNotificationStore,
            IOptions<SignalROptions> signalROptions)
        {
            this.userNotificationStore = userNotificationStore;

            this.signalROptions = signalROptions.Value;
        }

        [HttpPost("api/me/web/connect")]
        [AppPermission(NotifoRoles.AppUser)]
        public IActionResult GetMyConnection()
        {
            var response = new ConnectDto
            {
                PollingInterval = signalROptions.PollingInterval
            };

            switch (signalROptions.Enabled)
            {
                case true when signalROptions.Sticky:
                    response.ConnectionMode = ConnectionMode.SignalR;
                    break;
                case true:
                    response.ConnectionMode = ConnectionMode.SignalRSockets;
                    break;
                default:
                    response.ConnectionMode = ConnectionMode.Polling;
                    break;
            }

            return Ok(response);
        }

        [HttpPost("api/me/web/poll")]
        [AppPermission(NotifoRoles.AppUser)]
        public async Task<IActionResult> GetMyPolling([FromBody] PollRequest request)
        {
            var token = request.Token ?? default;

            if (token != default)
            {
                token = token.Minus(Duration.FromSeconds(10));
            }

            var details = new TrackingDetails
            {
                Channel = Providers.Web
            };

#pragma warning disable MA0040 // Flow the cancellation token
            if (request.Delivered?.Length > 0)
            {
                await userNotificationStore.TrackSeenAsync(request.Delivered, details);
            }

            if (request.Seen?.Length > 0)
            {
                await userNotificationStore.TrackSeenAsync(request.Seen, details);
            }

            if (request.Confirmed != null)
            {
                foreach (var id in request.Confirmed)
                {
                    await userNotificationStore.TrackConfirmedAsync(id, details);
                }
            }
#pragma warning restore MA0040 // Flow the cancellation token

            if (request.Deleted != null)
            {
                foreach (var id in request.Deleted)
                {
                    await userNotificationStore.DeleteAsync(id, HttpContext.RequestAborted);
                }
            }

            var notifications = await userNotificationStore.QueryAsync(App.Id, UserId, new UserNotificationQuery
            {
                After = token,
                Scope = UserNotificationQueryScope.All,
                Take = 100,
                TotalNeeded = false
            }, HttpContext.RequestAborted);

            var continuationToken = notifications.Select(x => x.Updated).OrderBy(x => x).LastOrDefault();

            if (continuationToken == default)
            {
                continuationToken = SystemClock.Instance.GetCurrentInstant();
            }

            var response = new PollResponse
            {
                ContinuationToken = continuationToken
            };

            foreach (var notification in notifications)
            {
                if (notification.IsDeleted)
                {
                    response.Deletions ??= new List<Guid>();
                    response.Deletions.Add(notification.Id);
                }
                else
                {
                    response.Notifications.Add(UserNotificationDto.FromDomainObject(notification));
                }
            }

            return Ok(response);
        }
    }
}
