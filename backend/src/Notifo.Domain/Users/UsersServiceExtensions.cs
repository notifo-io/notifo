// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;
using Notifo.Domain.Users;

namespace Microsoft.Extensions.DependencyInjection;

public static class UsersServiceExtensions
{
    public static void AddMyUsers(this IServiceCollection services)
    {
        services.AddSingletonAs<UserStore>()
            .As<IUserStore>().As<ICounterTarget>();

        services.AddRequestHandler<UserStore, UserCommand, User?>();
    }
}
