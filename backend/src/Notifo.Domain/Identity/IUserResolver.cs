// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notifo.Domain.Identity
{
    public interface IUserResolver
    {
        Task<(IUser? User, bool Created)> CreateUserIfNotExistsAsync(string email, bool invited = false);

        Task<IUser?> FindByIdOrEmailAsync(string idOrEmail);

        Task<IUser?> FindByIdAsync(string idOrEmail);

        Task<List<IUser>> QueryByEmailAsync(string email);

        Task<List<IUser>> QueryAllAsync();

        Task<Dictionary<string, IUser>> QueryManyAsync(string[] ids);
    }
}
