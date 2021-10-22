// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.KeyValueStore;
using Notifo.Infrastructure.KeyValueStore.MongoDb;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MongoDbKeyValueStoreServiceExtensions
    {
        public static void AddMyMongoDbKeyValueStore(this IServiceCollection services)
        {
            services.AddSingletonAs<MongoDbKeyValueStore>()
                .As<IKeyValueStore>();
        }
    }
}
