// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Domain;
using Notifo.Domain.Apps;
using Notifo.Domain.UserNotifications;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Tracking;

[OpenApiIgnore]
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
    public async Task<IActionResult> Seen(string id, [FromQuery] string? channel = null, [FromQuery] Guid configurationId = default)
    {
        var tokens = Enumerable.Repeat(TrackingToken.Parse(id, channel, configurationId), 1);

        await userNotificationService.TrackSeenAsync(tokens);

        return TrackingPixel();
    }

    [HttpGet]
    [HttpPut]
    [HttpPost]
    [Route("api/tracking/notifications/{id:notEmpty}/delivered")]
    public async Task<IActionResult> Delivered(string id, [FromQuery] string? channel = null, [FromQuery] Guid configurationId = default)
    {
        var tokens = Enumerable.Repeat(TrackingToken.Parse(id, channel, configurationId), 1);

        await userNotificationService.TrackDeliveredAsync(tokens);

        return TrackingPixel();
    }

    [HttpGet]
    [Route("api/tracking/notifications/{id:notEmpty}/confirm")]
    public async Task<IActionResult> Confirm(string id, [FromQuery] string? channel = null, [FromQuery] Guid configurationId = default)
    {
        var token = TrackingToken.Parse(id, channel, configurationId);

        await userNotificationService.TrackConfirmedAsync(token);

        var notification = await userNotificationStore.FindAsync(token.NotificationId, HttpContext.RequestAborted);

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

    [HttpPost]
    [Route("api/tracking/notifications/{id:notEmpty}/confirm")]
    public async Task<IActionResult> ConfirmPost(string id, [FromQuery] string? channel = null, [FromQuery] Guid configurationId = default)
    {
        var token = TrackingToken.Parse(id, channel, configurationId);

        await userNotificationService.TrackConfirmedAsync(token);

        return NoContent();
    }

    private IActionResult TrackingPixel()
    {
        var stream = typeof(TrackingController).Assembly.GetManifestResourceStream("Notifo.Areas.Api.Controllers.Tracking.TrackingPixel.png")!;

        return File(stream, "image/png");
    }
}
