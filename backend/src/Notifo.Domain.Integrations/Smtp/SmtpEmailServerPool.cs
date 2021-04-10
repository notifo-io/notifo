// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Notifo.Domain.Integrations.Smtp
{
    public sealed class SmtpEmailServerPool
    {
        private readonly IMemoryCache memoryCache;

        public SmtpEmailServerPool()
        {
            memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        }

        public SmtpEmailServer GetServer(SmtpOptions options)
        {
            var cacheKey = $"{options.Host}_{options.Username}_{options.Password}_{options.Host}";

            var found = memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var sender = new SmtpEmailServer(options);

                entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
                {
                    EvictionCallback = (key, value, reason, state) =>
                    {
                        sender.Dispose();
                    }
                });

                return sender;
            });

            return found;
        }
    }
}
