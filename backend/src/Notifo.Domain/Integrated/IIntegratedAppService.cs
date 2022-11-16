// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrated;

public interface IIntegratedAppService
{
    Task<string?> GetTokenAsync(string userId,
        CancellationToken ct = default);
}
