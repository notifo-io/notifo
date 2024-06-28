// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Integrations.Discord;

public static class DiscordServiceExtensions
{
    public static IServiceCollection AddIntegrationDiscord(this IServiceCollection services)
    {
        services.AddSingletonAs<DiscordIntegration>()
            .As<IIntegration>();

        services.AddSingletonAs<DiscordBotClientPool>()
            .AsSelf();

        return services;
    }
}
