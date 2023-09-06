// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Tracking.Dtos;
using Notifo.Domain;
using Notifo.Domain.Apps;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Areas.Api.Controllers.Tracking;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class TrackingController : Controller
{
    private readonly IAppStore appStore;
    private readonly IUserNotificationService userNotificationService;
    private readonly IUserNotificationStore userNotificationStore;

    public TrackingController(
        IAppStore appStore,
        IUserNotificationService userNotificationService,
        IUserNotificationStore userNotificationStore)
    {
        this.appStore = appStore;
        this.userNotificationService = userNotificationService;
        this.userNotificationStore = userNotificationStore;
    }

    [HttpGet]
    [HttpPut]
    [HttpPost]
    [Route("api/tracking/notifications/{id:notEmpty}/seen")]
    public async Task<IActionResult> Seen(string id, [FromQuery] TrackingQueryDto? request = null)
    {
        await userNotificationService.TrackSeenAsync(ParseToken(id, request));

        return TrackingPixel();
    }

    [HttpGet]
    [HttpPut]
    [HttpPost]
    [Route("api/tracking/notifications/{id:notEmpty}/delivered")]
    public async Task<IActionResult> Delivered(string id, [FromQuery] TrackingQueryDto? request = null)
    {
        await userNotificationService.TrackDeliveredAsync(ParseToken(id, request));

        return TrackingPixel();
    }

    [HttpPost]
    [Route("api/tracking/notifications/{id:notEmpty}/confirm")]
    public async Task<IActionResult> ConfirmPost(string id, [FromQuery] TrackingQueryDto? request = null)
    {
        await userNotificationService.TrackConfirmedAsync(ParseToken(id, request));

        return NoContent();
    }

    [HttpGet]
    [Route("api/tracking/notifications/{id:notEmpty}/confirm")]
    public async Task<IActionResult> Confirm(string id, [FromQuery] TrackingQueryDto? request = null)
    {
        var token = ParseToken(id, request);

        await userNotificationService.TrackConfirmedAsync(token);

        var notification = await userNotificationStore.FindAsync(token.UserNotificationId, HttpContext.RequestAborted);

        if (notification == null)
        {
            return View();
        }

        static bool TryGetLink(string? url, string id, out IActionResult result)
        {
            result = null!;

            if (url == null || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return false;
            }

            result = new RedirectResult(url.AppendQueries("id", id));
            return true;
        }

        if (TryGetLink(notification.Formatting.ConfirmLink, id, out var redirect))
        {
            return redirect;
        }

        var app = await appStore.GetCachedAsync(notification.AppId, HttpContext.RequestAborted);

        if (TryGetLink(app?.ConfirmUrl, id, out redirect))
        {
            return redirect;
        }

        return View();
    }

    private static TrackingToken ParseToken(string id, TrackingQueryDto? request)
    {
        return TrackingToken.Parse(
            id,
            request?.Channel,
            request?.ConfigurationId ?? default,
            request?.DeviceIdentifier);
    }

    private IActionResult TrackingPixel()
    {
        var stream = typeof(TrackingController).Assembly.GetManifestResourceStream("Notifo.Areas.Api.Controllers.Tracking.TrackingPixel.png")!;

        return File(stream, "image/png");
    }
}
