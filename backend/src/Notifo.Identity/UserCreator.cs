// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Hosting;

namespace Notifo.Identity
{
    public sealed class UserCreator : IInitializable
    {
        private readonly IServiceProvider serviceProvider;
        private readonly NotifoIdentityUser[]? users;

        public int Order => 1000;

        public UserCreator(IServiceProvider serviceProvider, IOptions<NotifoIdentityOptions> options)
        {
            this.serviceProvider = serviceProvider;

            users = options.Value.Users;
        }

        public async Task InitializeAsync(
            CancellationToken ct)
        {
            if (users?.Length > 0)
            {
                await using (var scope = serviceProvider.CreateAsyncScope())
                {
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>()!;

                    foreach (var user in users)
                    {
                        if (user?.IsConfigured() == true)
                        {
                            try
                            {
                                var existing = await userService.FindByEmailAsync(user.Email, ct);

                                var passwordValues = new UserValues { Password = user.Password };

                                if (existing == null)
                                {
                                    existing = await userService.CreateAsync(user.Email, passwordValues, ct: ct);
                                }
                                else if (user.PasswordReset)
                                {
                                    await userService.UpdateAsync(existing.Id, passwordValues, ct: ct);
                                }

                                if (!string.IsNullOrWhiteSpace(user.Role))
                                {
                                    var values = new UserValues
                                    {
                                        Roles = new HashSet<string>
                                        {
                                            user.Role
                                        }
                                    };

                                    await userService.UpdateAsync(existing.Id, values, ct: ct);
                                }
                            }
                            catch (Exception ex)
                            {
                                var log = serviceProvider.GetRequiredService<ILogger<UserCreator>>();

                                log.LogError(ex, "Failed to create initial user with email {email}.", user.Email);
                            }
                        }
                    }
                }
            }
        }
    }
}
