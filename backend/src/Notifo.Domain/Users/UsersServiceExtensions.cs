// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;
using Notifo.Domain.Users;
using Notifo.Domain.Users.MongoDb;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UsersServiceExtensions
    {
        public static void AddMyUsers(this IServiceCollection services)
        {
            services.AddSingletonAs<UserStore>()
                .As<IUserStore>().As<ICounterTarget>();
        }

        public static void AddMyMongoUsers(this IServiceCollection services)
        {
            services.AddSingletonAs<MongoDbUserRepository>()
                .As<IUserRepository>();
        }
    }
}
