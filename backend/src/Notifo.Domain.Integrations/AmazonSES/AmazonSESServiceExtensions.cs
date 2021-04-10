// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.AmazonSES;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AmazonSESServiceExtensions
    {
        public static void AddIntegrationAmazonSES(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("email:type", new Alternatives
            {
                ["AmazonSES"] = () =>
                {
                    services.AddAmazonSES(config);
                },
                ["None"] = () =>
                {
                }
            });
        }

        private static void AddAmazonSES(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureAndValidate<AmazonSESOptions>(config, "email:amazonSES");

            services.AddSingletonAs<AmazonSESIntegration>()
                .As<IIntegration>();
        }
    }
}
