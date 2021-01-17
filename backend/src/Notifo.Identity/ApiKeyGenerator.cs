// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Identity
{
    public sealed class ApiKeyGenerator : IApiKeyGenerator
    {
        public Task<string> GenerateAppTokenAsync(string appId)
        {
            return Task.FromResult(RandomHash.New());
        }

        public Task<string> GenerateUserTokenAsync(string appId, string userId)
        {
            return Task.FromResult(RandomHash.New());
        }
    }
}
