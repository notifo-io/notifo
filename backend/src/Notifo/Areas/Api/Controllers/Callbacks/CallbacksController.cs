// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations;
using Notifo.Pipeline;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Callbacks
{
    [OpenApiIgnore]
    public sealed class CallbacksController : Controller
    {
        private readonly IAppStore appStore;

        public CallbacksController(IAppStore appStore)
        {
            this.appStore = appStore;
        }

        [AllowSynchronousIO]
        [HttpGet]
        [HttpPost]
        [Route("/api/callback/sms")]
        public async Task<IActionResult> SmsCallback([FromQuery] string appId, [FromQuery] string integrationId)
        {
            var (app, sender) = await GetIntegrationAsync<ISmsSender>(appId, integrationId);

            if (app == null || sender == null)
            {
                return NotFound();
            }

            await sender.HandleCallbackAsync(app, HttpContext);

            return Ok();
        }

        [AllowSynchronousIO]
        [HttpGet]
        [HttpPost]
        [Route("/api/callback/messaging")]
        public async Task<IActionResult> MessagingCallback([FromQuery] string appId, [FromQuery] string integrationId)
        {
            var (app, sender) = await GetIntegrationAsync<IMessagingSender>(appId, integrationId);

            if (app == null || sender == null)
            {
                return NotFound();
            }

            await sender.HandleCallbackAsync(app, HttpContext);

            return Ok();
        }

        private async Task<(App? App, T? Integration)> GetIntegrationAsync<T>(string appId, string id) where T : class
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return default;
            }

            var app = await appStore.GetCachedAsync(appId, HttpContext.RequestAborted);

            if (app == null)
            {
                return default;
            }

            var integrationManager = HttpContext.RequestServices.GetRequiredService<IIntegrationManager>();

            return (app, integrationManager.Resolve<T>(id, app));
        }
    }
}
