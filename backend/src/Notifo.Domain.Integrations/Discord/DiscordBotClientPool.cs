// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Discord;
using Microsoft.Extensions.Caching.Memory;

namespace Notifo.Domain.Integrations.Discord;

public class DiscordBotClientPool : CachePool<IDiscordClient>
{
    public DiscordBotClientPool(IMemoryCache memoryCache)
        : base(memoryCache)
    {
    }

    public async Task<IDiscordClient> GetDiscordClient(string botToken, CancellationToken ct)
    {
        var cacheKey = $"{nameof(IDiscordClient)}_{botToken}";

        var found = await GetOrCreateAsync(cacheKey, TimeSpan.FromMinutes(5), async () =>
        {
            var client = new DiscordClient();

            // Sadly it can't receive the cancellation token.
            // The LoginAsync method of DiscordRestClient is a wrapper over the internal ApiClient.LoginAsync function.
            // That one accepts the RequestOptions parameter, which has a CancellationToken property.
            // The problem is that the DiscordClient.LoginAsync function doesn't expose the RequestOptions parameter.
            // We could use workarounds like WaitAsync, but it wouldn't interrupt the operation anyway.
            // Killing the thread on the other hand would also be a bad idea and could cause some integrity issues.
            await client.LoginAsync(TokenType.Bot, botToken);

            return client;
        });

        return found;
    }
}
