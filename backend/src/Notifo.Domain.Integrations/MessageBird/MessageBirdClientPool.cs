// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Notifo.Domain.Integrations.MessageBird.Implementation;

namespace Notifo.Domain.Integrations.MessageBird;

public sealed class MessageBirdClientPool(IMemoryCache memoryCache, IHttpClientFactory httpClientFactory) : CachePool<IMessageBirdClient>(memoryCache)
{
    public IMessageBirdClient GetClient(string accessKey)
    {
        var cacheKey = $"MessageBirdSmsSender_{accessKey}";

        var found = GetOrCreate(cacheKey, () =>
        {
            var options = Options.Create(new MessageBirdOptions
            {
                AccessKey = accessKey
            });

            var sender = new MessageBirdClient(httpClientFactory, options);

            return sender;
        });

        return found;
    }
}
