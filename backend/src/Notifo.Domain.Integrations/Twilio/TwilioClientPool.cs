// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Twilio.Clients;

namespace Notifo.Domain.Integrations.Twilio;

public sealed class TwilioClientPool : CachePool<ITwilioRestClient>
{
    public TwilioClientPool(IMemoryCache memoryCache)
        : base(memoryCache)
    {
    }

    public ITwilioRestClient GetServer(string username, string password)
    {
        var cacheKey = $"TwilioRestClient_{username}_{password}";

        var found = GetOrCreate(cacheKey, () =>
        {
            var client = new TwilioRestClient(username, password);

            return client;
        });

        return found;
    }
}
