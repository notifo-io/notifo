// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Scheduling.Implementation;
using Notifo.Infrastructure.Scheduling.Implementation.TimerBased.MongoDb;

namespace Microsoft.Extensions.DependencyInjection;

public static class MongoDbSchedulerServiceExtensions
{
    public static void AddMyMongoDbScheduler(this IServiceCollection services)
    {
        services.AddSingletonAs<MongoDbSchedulingProvider>()
            .As<ISchedulingProvider>();
    }
}
