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
using Notifo.Domain.Channels;
using Notifo.Domain.Identity;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Web;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class WebController : BaseController
{
    private static readonly UserNotificationQuery DefaultQuery = new UserNotificationQuery { Take = 100 };

    private readonly IUserNotificationService userNotificationService;
    private readonly IUserNotificationStore userNotificationStore;
    private readonly SignalROptions signalROptions;

    public WebController(
        IUserNotificationService userNotificationService,
        IUserNotificationStore userNotificationStore,
        IOptions<SignalROptions> signalROptions)
    {
        this.userNotificationService = userNotificationService;
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

        if (request.Delivered?.Length > 0)
        {
            var tokens = Enumerable.Select(request.Delivered, x => TrackingToken.Parse(x)).ToArray();

            await userNotificationService.TrackDeliveredAsync(tokens);
        }

        if (request.Seen?.Length > 0)
        {
            var tokens = Enumerable.Select(request.Seen, x => TrackingToken.Parse(x)).ToArray();

            await userNotificationService.TrackSeenAsync(tokens);
        }

        if (request.Confirmed?.Length > 0)
        {
            var tokens = Enumerable.Select(request.Confirmed, x => TrackingToken.Parse(x)).ToArray();

            await userNotificationService.TrackConfirmedAsync(tokens);
        }

        foreach (var id in request.Deleted.OrEmpty())
        {
            // It is not worth to cancel the request here and stop the tracking.
            await userNotificationStore.DeleteAsync(id, default);
        }

        var notifications = await userNotificationStore.QueryAsync(App.Id, UserId, DefaultQuery with { After = requestToken }, HttpContext.RequestAborted);

        // Calculate the continuation token from the hightest update value.
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
                response.Notifications.Add(UserNotificationDto.FromDomainObject(notification, Providers.Web));
            }
        }

        return Ok(response);
    }
}
