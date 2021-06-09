// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Mailjet.Client;
using Microsoft.Extensions.Caching.Memory;

namespace Notifo.Domain.Integrations.Mailjet
{
    internal class MailjetEmailServerPool
    {
        private readonly IMemoryCache memoryCache;

        public MailjetEmailServerPool(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public MailjetEmailServer GetServer(string apiKey, string apiSecret)
        {
            var cacheKey = $"MailjetEmailServer_{apiKey}_{apiSecret}";

            var found = memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var sender =
                    new MailjetEmailServer(
                        new MailjetClient(apiKey, apiSecret));

                return sender;
            });

            return found;
        }
    }
}
