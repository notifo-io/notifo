// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CachingServiceExtensions
    {
        public static void AddMyCaching(this IServiceCollection services)
        {
            services.AddReplicatedCache(options =>
            {
                options.Enable = true;
            });

            services.AddAsyncLocalCache();
        }
    }
}
