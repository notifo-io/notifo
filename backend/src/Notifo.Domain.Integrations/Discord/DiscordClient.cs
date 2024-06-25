// =====================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  Author of the file: Artur Nowak
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Discord.Rest;

namespace Notifo.Domain.Integrations.Discord;
public class DiscordClient : DiscordRestClient, IAsyncDisposable
{
    public async new ValueTask DisposeAsync()
    {
        await LogoutAsync();
        await base.DisposeAsync();
    }
}
