// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Log;
using Notifo.Domain.Log.MongoDb;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LogServiceExtensions
    {
        public static void AddMyLog(this IServiceCollection services)
        {
            services.AddSingletonAs<MongoDbLogRepository>()
                .As<ILogRepository>();

            services.AddSingletonAs<LogStore>()
                .As<ILogStore>();
        }
    }
}
