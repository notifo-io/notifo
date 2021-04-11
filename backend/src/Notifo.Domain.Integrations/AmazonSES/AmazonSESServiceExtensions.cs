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
            const string key = "email:amazonSES";

            var options = config.GetSection(key).Get<AmazonSESOptions>();

            if (options.IsValid())
            {
                services.ConfigureAndValidate<AmazonSESOptions>(config, key);

                services.AddSingletonAs<IntegratedAmazonSESIntegration>()
                    .As<IIntegration>();
            }
        }
    }
}
