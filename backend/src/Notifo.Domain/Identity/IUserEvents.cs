// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Identity
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
