// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Identity;
using Notifo.Identity;

namespace Notifo.Pipeline;

public class AuthorizeHostAdminAttribute : AuthorizeUserAttribute
{
    public string[] RequiredAppRoles { get; }

    public AuthorizeHostAdminAttribute(params string[] roles)
    {
        AuthenticationSchemes = Constants.IdentityServerOrApiKeyScheme;

        Roles = NotifoRoles.HostAdmin;
    }
}
