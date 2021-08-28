// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Domain.Apps;
using Notifo.Domain.UserNotifications;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Tracking
{
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
        [Route("/api/tracking/notifications/{id}/seen")]
        public async Task<IActionResult> Seen(Guid id, [FromQuery] string? channel = null, [FromQuery] string? deviceIdentifier = null)
        {
            var details = new TrackingDetails(channel, deviceIdentifier);

            await userNotificationService.TrackSeenAsync(Enumerable.Repeat(id, 1), details);

            return TrackingPixel();
        }

        [HttpGet]
        [HttpPut]
        [HttpPost]
        [Route("/api/tracking/notifications/{id}/delivered")]
        public async Task<IActionResult> Delivered(Guid id, [FromQuery] string? channel = null, [FromQuery] string? deviceIdentifier = null)
        {
            var details = new TrackingDetails(channel, deviceIdentifier);

            await userNotificationService.TrackDeliveredAsync(Enumerable.Repeat(id, 1), details);

            return TrackingPixel();
        }

        [HttpGet("/api/tracking/notifications/{id}/confirm")]
        public async Task<IActionResult> Confirm(Guid id, [FromQuery] string? channel = null, [FromQuery] string? deviceIdentifier = null)
        {
            var details = new TrackingDetails(channel, deviceIdentifier);

            await userNotificationService.TrackConfirmedAsync(id, details);

            var notification = await userNotificationStore.FindAsync(id, HttpContext.RequestAborted);

            if (notification == null)
            {
                return View();
            }

            var app = await appStore.GetCachedAsync(notification.AppId, HttpContext.RequestAborted);

            if (app?.ConfirmUrl != null && Uri.IsWellFormedUriString(app.ConfirmUrl, UriKind.Absolute))
            {
                var url = app.ConfirmUrl!;

                if (url.Contains("?", StringComparison.OrdinalIgnoreCase))
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

        [HttpPost("/api/tracking/notifications/{id}/confirm")]
        public async Task<IActionResult> ConfirmPost(Guid id, [FromQuery] string? channel = null, [FromQuery] string? deviceIdentifier = null)
        {
            var details = new TrackingDetails(channel, deviceIdentifier);

            await userNotificationService.TrackConfirmedAsync(id, details);

            return NoContent();
        }

        private IActionResult TrackingPixel()
        {
            var stream = typeof(TrackingController).Assembly.GetManifestResourceStream("Notifo.Areas.Api.Controllers.Tracking.TrackingPixel.png")!;

            return File(stream, "image/png");
        }
    }
}
