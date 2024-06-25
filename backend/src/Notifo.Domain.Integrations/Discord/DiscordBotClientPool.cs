// =====================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  Author of the file: Artur Nowak
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

    public IDiscordClient GetDiscordClient(string botToken, CancellationToken ct)
    {
        var cacheKey = $"{nameof(IDiscordClient)}_{botToken}";

        var found = GetOrCreate(cacheKey, TimeSpan.FromMinutes(5), () =>
        {
            var client = new DiscordClient();

            client.LoginAsync(TokenType.Bot, botToken).Wait(ct);

            return client;
        });

        return found;
    }

}
