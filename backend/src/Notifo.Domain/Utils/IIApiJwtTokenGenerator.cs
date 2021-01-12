// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Notifo.Domain.Utils
{
    public interface IIApiJwtTokenGenerator
    {
        Task<string> GenerateAppTokenAsync(string appId);

        Task<string> GenerateUserTokenAsync(string appId, string userId);
    }
}
