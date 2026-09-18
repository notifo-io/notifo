// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Domain.Apps;
using Notifo.Domain.Integrations;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Callbacks;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class CallbacksController(IAppStore appStore) : BaseController
{
    [AllowSynchronousIO]
    [HttpGet]
    [HttpPost]
    [Route("api/callback")]
    public async Task<IActionResult> Callback([FromQuery] string appId, [FromQuery] string integrationId)
    {
        if (string.IsNullOrWhiteSpace(appId))
        {
            return NotFound();
        }

        var app = await appStore.GetCachedAsync(appId, HttpContext.RequestAborted);
        if (app == null)
        {
            return NotFound();
        }

        var integrationManager = HttpContext.RequestServices.GetRequiredService<IIntegrationManager>();

        await integrationManager.OnCallbackAsync(integrationId, app, HttpContext, default);

        // The integration can write its own response, which must not be overriden.
        if (HttpContext.Response.HasStarted)
        {
            return new EmptyResult();
        }

        return Ok();
    }
}
