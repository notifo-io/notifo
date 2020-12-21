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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Identity;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body

namespace Notifo.Identity
{
    public sealed class UserResolver : IUserResolver
    {
        private readonly IServiceProvider serviceProvider;

        public UserResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<string?> GetOrAddUserAsync(string email)
        {
            Guard.NotNullOrEmpty(email, nameof(email));

            using (var scope = serviceProvider.CreateScope())
            {
                var userFactory = scope.ServiceProvider.GetRequiredService<IUserFactory>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<NotifoUser>>();

                if (!email.Contains("@"))
                {
                    var userById = await userManager.FindByIdAsync(email);

                    return userById?.Id;
                }

                try
                {
                    var user = userFactory.CreateUser(email);

                    await userManager.CreateAsync(user);
                }
                catch
                {
                }

                var userByEmail = await userManager.FindByEmailAsync(email);

                return userByEmail?.Id;
            }
        }

        public Task<Dictionary<string, string>> GetUserNamesAsync(HashSet<string> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            using (var scope = serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<NotifoUser>>();
                var userFactory = scope.ServiceProvider.GetRequiredService<IUserFactory>();

                var users = userManager.Users.Where(x => ids.Contains(x.Id)).ToList();

                return Task.FromResult(users.ToDictionary(x => x.Id, x => x.UserName));
            }
        }
    }
}
