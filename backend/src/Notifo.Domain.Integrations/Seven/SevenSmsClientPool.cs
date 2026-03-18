// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Notifo.Domain.Integrations.Seven.Implementation;

namespace Notifo.Domain.Integrations.Seven;

public sealed class SevenSmsClientPool(IMemoryCache memoryCache, IHttpClientFactory httpClientFactory) : CachePool<ISevenSmsClient>(memoryCache)
{
    public ISevenSmsClient GetClient(string apiKey)
    {
        var cacheKey = $"SevenSmsSender_{apiKey}";

        var found = GetOrCreate(cacheKey, () =>
        {
            return new SevenSmsClient(apiKey, httpClientFactory);
        });

        return found;
    }
}
