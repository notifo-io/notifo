// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Identity
{
    public interface IUserFactory
    {
        NotifoUser CreateUser(string email);
    }
}
