// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Notifo.Domain.Integrations.MessageBird.Implementation;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class MessageBirdSmsSenderPool
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IMemoryCache memoryCache;

        public MessageBirdSmsSenderPool(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            this.httpClientFactory = httpClientFactory;

            this.memoryCache = memoryCache;
        }

        public MessageBirdSmsSender GetServer(string accessKey, string phoneNumber)
        {
            var cacheKey = $"MessageBirdSmsSender_{accessKey}_{phoneNumber}";

            var found = memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var options = Options.Create(new MessageBirdOptions
                {
                    AccessKey = accessKey,
                    PhoneNumber = phoneNumber,
                    PhoneNumbers = null
                });

                var sender = new MessageBirdSmsSender(new MessageBirdClient(httpClientFactory, options));

                return sender;
            });

            return found;
        }
    }
}
