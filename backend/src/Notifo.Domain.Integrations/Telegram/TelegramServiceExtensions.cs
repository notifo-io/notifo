// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Telegram;

namespace Microsoft.Extensions.DependencyInjection;

public static class TelegramServiceExtensions
{
    public static IServiceCollection AddIntegrationTelegram(this IServiceCollection services)
    {
        services.AddSingletonAs<TelegramIntegration>()
            .As<IIntegration>();

        services.AddSingletonAs<TelegramBotClientPool>()
            .AsSelf();

        return services;
    }
}
