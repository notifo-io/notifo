// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Notifo.Identity;

namespace Notifo.Pipeline;

public class AuthorizeUserAttribute : AuthorizeAttribute
{
    public AuthorizeUserAttribute()
    {
        AuthenticationSchemes = Constants.IdentityServerOrApiKeyScheme;
    }
}
