// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifo.Domain.Integrations;

namespace Notifo.Areas.Api.Controllers.Integrations;

public class IntegrationsController : Controller
{
    private readonly IIntegrationManager integrationManager;

    public IntegrationsController(IIntegrationManager integrationManager)
    {
        this.integrationManager = integrationManager;
    }

    [HttpGet("api/integrations/image/{type}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImageAsync(string type)
    {
        var image = await integrationManager.GetImageAsync(type, HttpContext.RequestAborted);

        if (image != null)
        {
            return File(image.Stream, image.ContentType);
        }

        return NotFound();
    }
}
