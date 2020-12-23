// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static void AddMyStorage(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("storage:type", new Alternatives
            {
                ["MongoDB"] = () =>
                {
                    services.AddMyMongoDb(config);
                    services.AddMyMongoDbIdentity();
                    services.AddMyMongoDbScheduler();
                }
            });
        }
    }
}
