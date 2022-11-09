// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Apps.MongoDb;
using Notifo.Domain.Counters;

namespace Microsoft.Extensions.DependencyInjection;

public static class AppsServiceExtensions
{
    public static void AddMyApps(this IServiceCollection services)
    {
        services.AddSingletonAs<AppStore>()
            .As<IAppStore>().As<ICounterTarget>();
    }

    public static void AddMyMongoApps(this IServiceCollection services)
    {
        services.AddSingletonAs<MongoDbAppRepository>()
            .As<IAppRepository>();
    }
}
