// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Identity;
using Notifo.Identity;

namespace Notifo.Pipeline;

public sealed class AuthorizeHostAdminAttribute : AuthorizeUserAttribute
{
    public AuthorizeHostAdminAttribute()
    {
        AuthenticationSchemes = Constants.IdentityServerOrApiKeyScheme;

        Roles = NotifoRoles.HostAdmin;
    }
}
