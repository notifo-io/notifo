// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Notifo.Infrastructure;

namespace Notifo.Identity;

public sealed class EFUserFactory : IUserFactory
{
    public IdentityUser Create(string email)
    {
        Guard.NotNullOrEmpty(email);

        return new IdentityUser { Email = email, UserName = email };
    }

    public bool IsId(string id)
    {
        return Guid.TryParse(id, CultureInfo.InvariantCulture, out _);
    }
}
