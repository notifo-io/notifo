// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Utils;

public interface IApiKeyGenerator
{
    Task<string> GenerateAppTokenAsync(string appId);

    Task<string> GenerateUserTokenAsync(string appId, string userId);
}
