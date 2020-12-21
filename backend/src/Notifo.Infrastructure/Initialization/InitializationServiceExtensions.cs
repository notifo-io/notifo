// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Hosting;
using Notifo.Infrastructure.Initialization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InitializationServiceExtensions
    {
        public static void AddMyInitialization(this IServiceCollection services)
        {
            services.AddSingletonAs<Initializer>()
                .As<IHostedService>();
        }
    }
}
