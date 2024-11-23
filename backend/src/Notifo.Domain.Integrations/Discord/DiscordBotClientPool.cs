// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Discord;
using Microsoft.Extensions.Caching.Memory;

namespace Notifo.Domain.Integrations.Discord;

public class DiscordBotClientPool(IMemoryCache memoryCache) : CachePool<IDiscordClient>(memoryCache)
{
    public async Task<IDiscordClient> GetDiscordClient(string botToken)
    {
        var cacheKey = $"{nameof(IDiscordClient)}_{botToken}";

        var found = await GetOrCreateAsync(cacheKey, TimeSpan.FromMinutes(5), async () =>
        {
            var client = new DiscordClient();

            // Method provides no option to pass CancellationToken
            await client.LoginAsync(TokenType.Bot, botToken);

            return client;
        });

        return found;
    }
}
