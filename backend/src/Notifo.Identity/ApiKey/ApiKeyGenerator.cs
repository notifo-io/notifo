// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Utils;
using Notifo.Infrastructure;

namespace Notifo.Identity.ApiKey;

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
