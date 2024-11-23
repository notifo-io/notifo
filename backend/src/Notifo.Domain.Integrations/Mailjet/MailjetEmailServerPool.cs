// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Mailjet.Client;
using Microsoft.Extensions.Caching.Memory;

namespace Notifo.Domain.Integrations.Mailjet;

public sealed class MailjetEmailServerPool(IMemoryCache memoryCache) : CachePool<MailjetEmailServer>(memoryCache)
{
    public MailjetEmailServer GetServer(string apiKey, string apiSecret)
    {
        var cacheKey = $"MailjetEmailServer_{apiKey}_{apiSecret}";

        var found = GetOrCreate(cacheKey, () =>
        {
            var sender = new MailjetEmailServer(new MailjetClient(apiKey, apiSecret));

            return sender;
        });

        return found;
    }
}
