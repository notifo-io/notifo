// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;

namespace Notifo.Domain.Integrations.Firebase
{
    public sealed class FirebaseMessagingPool
    {
        private readonly IMemoryCache memoryCache;

        public FirebaseMessagingPool(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public FirebaseMessagingWrapper GetMessaging(string projectId, string credentials)
        {
            var cacheKey = $"FirebaseSender_{projectId}_{credentials}";

            var found = memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var sender = new FirebaseMessagingWrapper(projectId, credentials);

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
