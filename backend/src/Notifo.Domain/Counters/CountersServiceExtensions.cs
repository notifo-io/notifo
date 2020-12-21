// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Counters
{
    public static class CountersServiceExtensions
    {
        public static void AddMyCounters(this IServiceCollection services)
        {
            services.AddSingletonAs<CounterService>()
                .As<ICounterService>();
        }
    }
}
