// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Domain.Identity;

public sealed class UserUpdated
{
    public IUser User { get; set; }

    public IUser OldUser { get; set; }
}

public sealed class UserRegistered
{
    public IUser User { get; set; }
}

public sealed class UserConsentGiven
{
    public IUser User { get; set; }
}

public sealed class UserDeleted
{
    public IUser User { get; set; }
}
