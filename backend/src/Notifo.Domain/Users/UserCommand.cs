// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Users;

public abstract class UserCommand : AppCommandBase<User>
{
    public string UserId { get; set; }
}
