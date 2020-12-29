// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Hosting;
using Squidex.Log;

namespace Notifo.Identity
{
    public sealed class UserCreator : IInitializable
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ISemanticLog log;
        private readonly NotifoIdentityUser[]? users;

        public int Order => 1000;

        public UserCreator(IServiceProvider serviceProvider, IOptions<NotifoIdentityOptions> options, ISemanticLog log)
        {
            this.serviceProvider = serviceProvider;

            users = options.Value.Users;

            this.log = log;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (users != null && users.Length > 0)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<NotifoUser>>()!;
                    var userFactory = scope.ServiceProvider.GetRequiredService<IUserFactory>()!;

                    async Task DoAsync(Func<UserManager<NotifoUser>, Task<IdentityResult>> handler)
                    {
                        var result = await handler(userManager!);

                        if (!result.Succeeded)
                        {
                            var error = string.Join(" ", result.Errors.Select(x => $"[{x.Code}]: {x.Description}."));

                            throw new InvalidOperationException(error);
                        }
                    }

                    foreach (var user in users)
                    {
                        if (user?.IsConfigured() == true)
                        {
                            try
                            {
                                var existing = await userManager.FindByEmailAsync(user.Email);

                                if (existing == null)
                                {
                                    existing = userFactory.CreateUser(user.Email);

                                    await DoAsync(m => m.CreateAsync(existing));
                                    await DoAsync(m => m.AddPasswordAsync(existing, user.Password));
                                }
                                else if (user.PasswordReset)
                                {
                                    if (await userManager.HasPasswordAsync(existing))
                                    {
                                        await DoAsync(m => m.RemovePasswordAsync(existing));
                                    }

                                    await DoAsync(m => m.AddPasswordAsync(existing, user.Password));
                                }

                                if (!string.IsNullOrWhiteSpace(user.Role) && !await userManager.IsInRoleAsync(existing, user.Role))
                                {
                                    await DoAsync(m => m.AddToRoleAsync(existing, user.Role));
                                }
                            }
                            catch (Exception ex)
                            {
                                log.LogError(ex, w => w
                                    .WriteProperty("action", "CreateUser")
                                    .WriteProperty("status", "Failed")
                                    .WriteProperty("email", user.Email));
                            }
                        }
                    }
                }
            }
        }
    }
}
