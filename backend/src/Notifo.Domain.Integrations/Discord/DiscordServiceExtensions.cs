// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Discord;

namespace Microsoft.Extensions.DependencyInjection;

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
