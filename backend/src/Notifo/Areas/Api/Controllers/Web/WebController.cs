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
using Notifo.Domain;
using Notifo.Domain.Identity;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Web
{
    [OpenApiIgnore]
    public sealed class WebController : BaseController
    {
        private static readonly UserNotificationQuery DefaultQuery = new UserNotificationQuery { Take = 100 };

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
            var requestToken = request.Token ?? default;

            if (requestToken != default)
            {
                requestToken = requestToken.Minus(Duration.FromSeconds(10));
            }

#pragma warning disable MA0040 // Flow the cancellation token
            if (request.Delivered?.Length > 0)
            {
                var tokens = request.Delivered.Select(x => TrackingToken.Parse(x));

                await userNotificationStore.TrackSeenAsync(tokens);
            }

            if (request.Seen?.Length > 0)
            {
                var tokens = request.Seen.Select(x => TrackingToken.Parse(x));

                await userNotificationStore.TrackSeenAsync(tokens);
            }

            foreach (var id in request.Confirmed.OrEmpty())
            {
                var token = TrackingToken.Parse(id);

                await userNotificationStore.TrackConfirmedAsync(token);
            }
#pragma warning restore MA0040 // Flow the cancellation token

            foreach (var id in request.Deleted.OrEmpty())
            {
                await userNotificationStore.DeleteAsync(id, HttpContext.RequestAborted);
            }

            var notifications = await userNotificationStore.QueryAsync(App.Id, UserId, DefaultQuery with { After = requestToken }, HttpContext.RequestAborted);

            var continuationToken = notifications.Select(x => x.Updated).OrderBy(x => x).LastOrDefault();

            if (continuationToken == default)
            {
                continuationToken = SystemClock.Instance.GetCurrentInstant();
            }

            var response = new PollResponse
            {
                ContinuationToken = continuationToken
            };

            foreach (var notification in notifications.OrEmpty().NotNull())
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
