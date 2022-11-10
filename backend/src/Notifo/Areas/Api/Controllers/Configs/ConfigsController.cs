// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Areas.Api.Controllers.Configs;

[ApiExplorerSettings(GroupName = "Configs")]
public class ConfigsController : BaseController
{
    /// <summary>
    /// Get all supported timezones.
    /// </summary>
    /// <response code="200">Timezones returned.</response>.
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
    /// <response code="200">Languages returned.</response>.
    [HttpGet("api/languages")]
    [Produces(typeof(string[]))]
    public IActionResult GetLanguages()
    {
        var response = Language.AllLanguages.Select(x => x.Iso2Code).OrderBy(x => x);

        return Ok(response);
    }
}
