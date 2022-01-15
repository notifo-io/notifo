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
        Task OnUserRegisteredAsync(IUser user)
        {
            return Task.CompletedTask;
        }

        Task OnUserUpdatedAsync(IUser user, IUser previous)
        {
            return Task.CompletedTask;
        }

        Task OnUserDeletedAsync(IUser user)
        {
            return Task.CompletedTask;
        }

        Task OnConsentGivenAsync(IUser user)
        {
            return Task.CompletedTask;
        }
    }
}
