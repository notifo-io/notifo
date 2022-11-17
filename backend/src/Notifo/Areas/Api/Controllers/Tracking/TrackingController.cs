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

        var app = await appStore.GetCachedAsync(notification.AppId, HttpContext.RequestAborted);

        if (app?.ConfirmUrl != null && Uri.IsWellFormedUriString(app.ConfirmUrl, UriKind.Absolute))
        {
            var url = app.ConfirmUrl!;

            if (url.Contains('?', StringComparison.OrdinalIgnoreCase))
            {
                url += $"&id={id}";
            }
            else
            {
                url += $"?id={id}";
            }

            return Redirect(url);
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
