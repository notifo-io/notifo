// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        public async Task<(IUser? User, bool Created)> CreateUserIfNotExistsAsync(string emailOrId, bool invited = false,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(emailOrId, nameof(emailOrId));

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var found = await FindByIdOrEmailAsync(emailOrId, ct);

                if (found != null)
                {
                    return (found, false);
                }

                if (!emailOrId.Contains('@', StringComparison.OrdinalIgnoreCase))
                {
                    return (null, false);
                }

                try
                {
                    var user = await userService.CreateAsync(emailOrId, new UserValues
                    {
                        Invited = true
                    }, ct: ct);

                    return (user, true);
                }
                catch
                {
                }

                found = await FindByIdOrEmailAsync(emailOrId, ct);

                return (found, false);
            }
        }

        public async Task<IUser?> FindByIdAsync(string id,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                return await userService.FindByIdAsync(id, ct);
            }
        }

        public async Task<IUser?> FindByIdOrEmailAsync(string idOrEmail,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(idOrEmail, nameof(idOrEmail));

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                if (idOrEmail.Contains('@', StringComparison.OrdinalIgnoreCase))
                {
                    return await userService.FindByEmailAsync(idOrEmail, ct);
                }
                else
                {
                    return await userService.FindByIdAsync(idOrEmail, ct);
                }
            }
        }

        public async Task<List<IUser>> QueryAllAsync(
            CancellationToken ct = default)
        {
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var result = await userService.QueryAsync(take: int.MaxValue, ct: ct);

                return result.ToList();
            }
        }

        public async Task<List<IUser>> QueryByEmailAsync(string email,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(email, nameof(email));

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var result = await userService.QueryAsync(email, ct: ct);

                return result.ToList();
            }
        }

        public async Task<Dictionary<string, IUser>> QueryManyAsync(string[] ids,
            CancellationToken ct = default)
        {
            Guard.NotNull(ids, nameof(ids));

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var result = await userService.QueryAsync(ids, ct);

                return result.ToDictionary(x => x.Id);
            }
        }
    }
}
