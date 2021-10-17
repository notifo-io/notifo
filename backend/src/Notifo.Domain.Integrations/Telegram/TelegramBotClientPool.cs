// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;

namespace Notifo.Domain.Integrations.Telegram
{
    public sealed class TelegramBotClientPool : CachePool<ITelegramBotClient>
    {
        public TelegramBotClientPool(IMemoryCache memoryCache)
            : base(memoryCache)
        {
        }

        public ITelegramBotClient GetBotClient(string accessToken)
        {
            var cacheKey = $"{nameof(TelegramBotClient)}_{accessToken}";

            var found = GetOrCreate(cacheKey, TimeSpan.FromMinutes(5), () =>
            {
                var botClient = new TelegramBotClient(accessToken);

                return botClient;
            });

            return found;
        }
    }
}
