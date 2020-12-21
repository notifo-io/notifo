// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Domain.Channels.Sms;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Callbacks
{
    [OpenApiIgnore]
    public sealed class CallbacksController : Controller
    {
        private readonly ISmsSender smsSender;

        public CallbacksController(ISmsSender smsSender)
        {
            this.smsSender = smsSender;
        }

        [HttpGet]
        [HttpPost]
        [Route("/api/callback/sms")]
        public async Task<IActionResult> PostCallback()
        {
            await smsSender.HandleStatusAsync(HttpContext);

            return Ok();
        }
    }
}
