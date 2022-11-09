// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;

namespace Notifo.Domain.Integrations.Firebase;

public sealed class FirebaseMessagingPool : CachePool<FirebaseMessagingWrapper>
{
    public FirebaseMessagingPool(IMemoryCache memoryCache)
        : base(memoryCache)
    {
    }

    public FirebaseMessagingWrapper GetMessaging(string projectId, string credentials)
    {
        var cacheKey = $"FirebaseSender_{projectId}_{credentials}";

        var found = GetOrCreate(cacheKey, () =>
        {
            var sender = new FirebaseMessagingWrapper(projectId, credentials);

            return sender;
        });

        return found;
    }
}
