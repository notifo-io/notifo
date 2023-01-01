// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;

namespace Notifo.Domain.Integrations;

public interface IIntegrationHook
{
    Task HandleRequestAsync(AppContext app, HttpContext httpContext);
}
