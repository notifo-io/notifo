﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Notifo.Domain.Integrations.Firebase
{
    public sealed class FirebaseMobilePushSenderPool
    {
        private readonly IMemoryCache memoryCache;

        public FirebaseMobilePushSenderPool()
        {
            memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        }

        public FirebaseMobilePushSender GetSender(string projectId, string credentials)
        {
            var cacheKey = $"{projectId}_{credentials}";

            var found = memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var sender = new FirebaseMobilePushSender(projectId, credentials);

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