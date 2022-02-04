// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.MessageBird;
using Notifo.Domain.Integrations.MessageBird.Implementation;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MessageBirdServiceExtensions
    {
        public static void IntegrateMessageBird(this IServiceCollection services, IConfiguration config)
        {
            const string key = "sms:messageBird";

            var options = config.GetSection(key).Get<MessageBirdOptions>();

            if (options.IsValid())
            {
                services.ConfigureAndValidate<MessageBirdOptions>(config, key);

                services.AddSingletonAs<MessageBirdClient>()
                    .As<IMessageBirdClient>();

                services.AddSingletonAs<IntegratedMessageBirdIntegration>()
                    .As<IIntegration>();
            }

            services.AddSingletonAs<MessageBirdIntegration>()
                .As<IIntegration>();

            services.AddSingletonAs<MessageBirdClientPool>()
                .AsSelf();
        }
    }
}
