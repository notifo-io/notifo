// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Notifo.Infrastructure;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.Configs
{
    [OpenApiTag("Configs")]
    public class ConfigsController : BaseController
    {
        /// <summary>
        /// Get all supported timezones.
        /// </summary>
        /// <returns>
        /// 200 => Timezones returned.
        /// </returns>
        [HttpGet("api/timezones")]
        [Produces(typeof(string[]))]
        public IActionResult GetTimezones()
        {
            var response = DateTimeZoneProviders.Tzdb.Ids;

            return Ok(response);
        }

        /// <summary>
        /// Get all supported languages.
        /// </summary>
        /// <returns>
        /// 200 => Languages returned.
        /// </returns>
        [HttpGet("api/languages")]
        [Produces(typeof(string[]))]
        public IActionResult GetLanguages()
        {
            var response = Language.AllLanguages.Select(x => x.Iso2Code).OrderBy(x => x);

            return Ok(response);
        }
    }
}
