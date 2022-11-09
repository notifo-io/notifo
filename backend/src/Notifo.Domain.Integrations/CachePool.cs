// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;

namespace Notifo.Domain.Integrations;

public abstract class CachePool<TItem>
{
#pragma warning disable RECS0108 // Warns about static fields in generic types
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);
#pragma warning restore RECS0108 // Warns about static fields in generic types
    private readonly IMemoryCache memoryCache;

    protected CachePool(IMemoryCache memoryCache)
    {
        this.memoryCache = memoryCache;
    }

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
                                asyncDisposable.DisposeAsync();
#pragma warning restore CA2012 // Use ValueTasks correctly
                            }
                        });
                        break;
                    }
            }

            return item;
        });
    }
}
