﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.MessageBird;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MessageBirdServiceExtensions
    {
        public static void AddIntegrationMessageBird(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureAndValidate<MessageBirdOptions>(config, "messageBird");

            services.AddSingletonAs<MessageBirdClient>()
                .AsSelf();

            services.AddSingletonAs<IntegratedMessageBirdIntegration>()
                .As<IIntegration>();
        }
    }
}
