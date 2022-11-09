// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;

namespace Microsoft.Extensions.DependencyInjection;

public static class CountersServiceExtensions
{
    public static void AddMyCounters(this IServiceCollection services)
    {
        services.AddSingletonAs<CounterService>()
            .As<ICounterService>();
    }
}
