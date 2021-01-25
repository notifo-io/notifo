// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Notifo.Domain.Identity;
using Notifo.Infrastructure;

namespace Notifo.Identity
{
    public interface IUserService
    {
        Task<IResultList<IUser>> QueryAsync(IEnumerable<string> ids);

        Task<IResultList<IUser>> QueryAsync(string? query = null, int take = 10, int skip = 0);

        string GetUserId(ClaimsPrincipal user);

        Task<IList<UserLoginInfo>> GetLoginsAsync(IUser user);

        Task<bool> HasPasswordAsync(IUser user);

        Task<bool> IsEmptyAsync();

        Task<IUser> CreateAsync(string email, UserValues? values = null, bool lockAutomatically = false);

        Task<IUser?> GetAsync(ClaimsPrincipal principal);

        Task<IUser?> FindByEmailAsync(string email);

        Task<IUser?> FindByIdAsync(string id);

        Task<IUser?> FindByLoginAsync(string provider, string key);

        Task<IUser> SetPasswordAsync(string id, string password, string? oldPassword = null);

        Task<IUser> AddLoginAsync(string id, ExternalLoginInfo externalLogin);

        Task<IUser> RemoveLoginAsync(string id, string loginProvider, string providerKey);

        Task<IUser> LockAsync(string id);

        Task<IUser> UnlockAsync(string id);

        Task<IUser> UpdateAsync(string id, UserValues values);

        Task DeleteAsync(string id);
    }
}
