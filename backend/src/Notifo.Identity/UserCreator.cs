// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
                                var existing = await userService.FindByEmailAsync(user.Email);

                                var passwordValues = new UserValues { Password = user.Password };

                                if (existing == null)
                                {
                                    existing = await userService.CreateAsync(user.Email, passwordValues);
                                }
                                else if (user.PasswordReset)
                                {
                                    await userService.UpdateAsync(existing.Id, passwordValues);
                                }

                                if (!string.IsNullOrWhiteSpace(user.Role))
                                {
                                    await userService.UpdateAsync(existing.Id, new UserValues { Role = user.Role });
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
