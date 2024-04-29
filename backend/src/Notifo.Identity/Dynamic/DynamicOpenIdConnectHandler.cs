// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Notifo.Identity.Dynamic;

public sealed class DynamicOpenIdConnectHandler : OpenIdConnectHandler
{
    public DynamicOpenIdConnectHandler(IOptionsMonitor<DynamicOpenIdConnectOptions> options, ILoggerFactory logger, HtmlEncoder htmlEncoder, UrlEncoder encoder)
        : base(options, logger, htmlEncoder, encoder)
    {
    }
}
