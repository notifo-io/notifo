// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notifo.Infrastructure.Identity
{
    public interface IUserResolver
    {
        Task<string?> GetOrAddUserAsync(string email);

        Task<Dictionary<string, string>> GetUserNamesAsync(HashSet<string> ids);
    }
}
