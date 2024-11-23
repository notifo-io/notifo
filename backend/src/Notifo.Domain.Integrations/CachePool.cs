// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;

namespace Notifo.Domain.Integrations;

public abstract class CachePool<TItem>(IMemoryCache memoryCache)
{
#pragma warning disable RECS0108 // Warns about static fields in generic types
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

#pragma warning restore RECS0108 // Warns about static fields in generic types

    protected TItem GetOrCreate(object key, Func<TItem> factory)
    {
        return GetOrCreate(key, DefaultExpiration, factory);
    }

    protected TItem GetOrCreate(object key, TimeSpan expiration, Func<TItem> factory)
    {
        return memoryCache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = expiration;

            var item = factory();
            HandleDispose(item, entry);

            return item;
        })!;
    }

    protected Task<TItem> GetOrCreateAsync(object key, Func<Task<TItem>> factory)
    {
        return GetOrCreateAsync(key, DefaultExpiration, factory);
    }

    protected Task<TItem> GetOrCreateAsync(object key, TimeSpan expiration, Func<Task<TItem>> factory)
    {
        return memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = expiration;

            var item = await factory();
            HandleDispose(item, entry);

            return item;
        })!;
    }

    private void HandleDispose(TItem item, ICacheEntry entry)
    {
        switch (item)
        {
            case IDisposable disposable:
                {
                    entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (key, value, reason, state) =>
                        {
                            disposable.Dispose();
                        }
                    });
                    break;
                }

            case IAsyncDisposable asyncDisposable:
                {
                    entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (key, value, reason, state) =>
                        {
#pragma warning disable CA2012 // Use ValueTasks correctly
#pragma warning disable MA0134 // Observe result of async calls
                            asyncDisposable.DisposeAsync();
#pragma warning restore MA0134 // Observe result of async calls
#pragma warning restore CA2012 // Use ValueTasks correctly
                        }
                    });
                    break;
                }
        }
    }
}
