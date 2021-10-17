// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.Telegram;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TelegramServiceExtensions
    {
        public static void IntegrateTelegram(this IServiceCollection services)
        {
            services.AddSingletonAs<TelegramIntegration>()
                .As<IIntegration>();

            services.AddSingletonAs<TelegramBotClientPool>()
                .AsSelf();
        }
    }
}
