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
using Notifo.Domain.UserNotifications;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Tracking
{
    [OpenApiIgnore]
    public sealed class TrackingController : Controller
    {
        private readonly IUserNotificationService userNotificationService;

        public TrackingController(IUserNotificationService userNotificationsStore)
        {
            this.userNotificationService = userNotificationsStore;
        }

        [HttpGet("/api/tracking/notifications/{id}/seen")]
        public async Task<IActionResult> Seen(Guid id, [FromQuery] string? channel = null, [FromQuery] string? deviceIdentifier = null)
        {
            await userNotificationService.TrackSeenAsync(Enumerable.Repeat(id, 1), channel, deviceIdentifier);

            return TrackingPixel();
        }

        [HttpPost("/api/tracking/notifications/{id}/seen")]
        public async Task<IActionResult> SeenPost(Guid id, [FromQuery] string? channel = null, [FromQuery] string? deviceIdentifier = null)
        {
            await userNotificationService.TrackSeenAsync(Enumerable.Repeat(id, 1), channel, deviceIdentifier);

            return NoContent();
        }

        [HttpGet("/api/tracking/notifications/{id}/confirm")]
        public async Task<IActionResult> Confirm(Guid id, [FromQuery] string? channel = null)
        {
            var (_, app) = await userNotificationService.TrackConfirmedAsync(id, channel);

            if (app?.ConfirmUrl != null && Uri.IsWellFormedUriString(app.ConfirmUrl, UriKind.Absolute))
            {
                var url = app.WebhookUrl!;

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
        public async Task<IActionResult> ConfirmPost(Guid id, [FromQuery] string? channel = null)
        {
            await userNotificationService.TrackConfirmedAsync(id, channel);

            return NoContent();
        }

        private IActionResult TrackingPixel()
        {
            var stream = typeof(TrackingController).Assembly.GetManifestResourceStream($"Notifo.Areas.Api.Controllers.Tracking.TrackingPixel.png")!;

            return File(stream, "image/png");
        }
    }
}
