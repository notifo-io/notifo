// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Notifo.Domain.Integrations.MessageBird.Implementation;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class MessageBirdClientPool : CachePool<MessageBirdClient>
    {
        private readonly IHttpClientFactory httpClientFactory;

        public MessageBirdClientPool(IMemoryCache memoryCache, IHttpClientFactory httpClientFactory)
            : base(memoryCache)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public MessageBirdClient GetServer(string accessKey, long phoneNumber)
        {
            var cacheKey = $"MessageBirdSmsSender_{accessKey}_{phoneNumber}";

            var found = GetOrCreate(cacheKey, () =>
            {
                var options = Options.Create(new MessageBirdOptions
                {
                    AccessKey = accessKey,
                    PhoneNumber = phoneNumber.ToString(CultureInfo.InvariantCulture),
                    PhoneNumbers = null
                });

                var sender = new MessageBirdClient(httpClientFactory, options);

                return sender;
            });

            return found;
        }
    }
}
