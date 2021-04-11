// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Identity;
using Notifo.Infrastructure;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body

namespace Notifo.Identity
{
    public sealed class DefaultUserResolver : IUserResolver
    {
        private readonly IServiceProvider serviceProvider;

        public DefaultUserResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<(IUser? User, bool Created)> CreateUserIfNotExistsAsync(string email, bool invited)
        {
            Guard.NotNullOrEmpty(email, nameof(email));

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                try
                {
                    var user = await userService.CreateAsync(email, new UserValues { Invited = true });

                    return (user, true);
                }
                catch
                {
                }

                var found = await FindByIdOrEmailAsync(email);

                return (found, false);
            }
        }

        public async Task<IUser?> FindByIdAsync(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                return await userService.FindByIdAsync(id);
            }
        }

        public async Task<IUser?> FindByIdOrEmailAsync(string idOrEmail)
        {
            Guard.NotNullOrEmpty(idOrEmail, nameof(idOrEmail));

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                if (idOrEmail.Contains("@"))
                {
                    return await userService.FindByEmailAsync(idOrEmail);
                }
                else
                {
                    return await userService.FindByIdAsync(idOrEmail);
                }
            }
        }

        public async Task<List<IUser>> QueryAllAsync()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var result = await userService.QueryAsync(take: int.MaxValue);

                return result.ToList();
            }
        }

        public async Task<List<IUser>> QueryByEmailAsync(string email)
        {
            Guard.NotNullOrEmpty(email, nameof(email));

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var result = await userService.QueryAsync(email);

                return result.ToList();
            }
        }

        public async Task<Dictionary<string, IUser>> QueryManyAsync(string[] ids)
        {
            Guard.NotNull(ids, nameof(ids));

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var result = await userService.QueryAsync(ids);

                return result.ToDictionary(x => x.Id);
            }
        }
    }
}
