// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Users;
using Notifo.Infrastructure;

namespace Notifo.Domain.Liquid;

public sealed class LiquidUser
{
    private readonly User user;

    public string? FullName => user.FullName.OrNull();

    public string? EmailAddress => user.EmailAddress.OrNull();

    public string? PhoneNumber => user.PhoneNumber.OrNull();

    public LiquidUser(User user)
    {
        this.user = user;
    }
}
