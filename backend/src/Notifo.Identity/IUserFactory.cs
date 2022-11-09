// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;

namespace Notifo.Identity;

public interface IUserFactory
{
    IdentityUser Create(string email);

    bool IsId(string id);
}
