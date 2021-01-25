// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Identity;

namespace Notifo.Identity
{
    public interface IUserEvents
    {
        void OnUserRegistered(IUser user)
        {
        }

        void OnUserUpdated(IUser user)
        {
        }

        void OnUserDeleted(IUser user)
        {
        }

        void OnConsentGiven(IUser user)
        {
        }
    }
}
