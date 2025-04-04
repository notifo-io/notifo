﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Identity;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Mediator;
using Notifo.Infrastructure.Tasks;

namespace Notifo.Identity;

public sealed class DefaultUserService(
    UserManager<IdentityUser> userManager,
    IUserFactory userFactory,
    IMediator mediator,
    ILogger<DefaultUserService> log)
    : IUserService
{
    public async Task<bool> IsEmptyAsync(
        CancellationToken ct = default)
    {
        var result = await QueryAsync(null, 1, 0, ct);

        return result.Total == 0;
    }

    public string GetUserId(ClaimsPrincipal user,
        CancellationToken ct = default)
    {
        Guard.NotNull(user);

        return userManager.GetUserId(user)!;
    }

    public async IAsyncEnumerable<IUser> StreamAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var user in userManager.Users.ToAsyncEnumerable().WithCancellation(ct))
        {
            yield return await ResolveAsync(user);
        }
    }

    public async Task<IResultList<IUser>> QueryAsync(IEnumerable<string> ids,
        CancellationToken ct = default)
    {
        Guard.NotNull(ids);

        ids = ids.Where(userFactory.IsId);

        if (!ids.Any())
        {
            return ResultList.CreateFrom<IUser>(0);
        }

        var userItems = userManager.Users.Where(x => ids.Contains(x.Id)).ToList();
        var userTotal = userItems.Count;

        var resolved = await ResolveAsync(userItems);

        return ResultList.Create(userTotal, resolved);
    }

    public async Task<IResultList<IUser>> QueryAsync(string? query = null, int take = 10, int skip = 0,
        CancellationToken ct = default)
    {
        Guard.GreaterThan(take, 0);
        Guard.GreaterEquals(skip, 0);

        IQueryable<IdentityUser> QueryUsers(string? email = null)
        {
            var result = userManager.Users;

            if (!string.IsNullOrWhiteSpace(email))
            {
                var normalizedEmail = userManager.NormalizeEmail(email);

                result = result.Where(x => x.NormalizedEmail!.Contains(normalizedEmail));
            }

            return result;
        }

        var userItems = QueryUsers(query).Skip(skip).Take(take).ToList();
        var userTotal = QueryUsers(query).LongCount();

        var resolved = await ResolveAsync(userItems);

        return ResultList.Create(userTotal, resolved);
    }

    public Task<IList<UserLoginInfo>> GetLoginsAsync(IUser user,
        CancellationToken ct = default)
    {
        Guard.NotNull(user);

        return userManager.GetLoginsAsync((IdentityUser)user.Identity);
    }

    public Task<bool> HasPasswordAsync(IUser user,
        CancellationToken ct = default)
    {
        Guard.NotNull(user);

        return userManager.HasPasswordAsync((IdentityUser)user.Identity);
    }

    public async Task<IUser?> FindByLoginAsync(string provider, string key,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(provider);

        var user = await userManager.FindByLoginAsync(provider, key);

        return await ResolveOptionalAsync(user);
    }

    public async Task<IUser?> FindByEmailAsync(string email,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(email);

        var user = await userManager.FindByEmailAsync(email);

        return await ResolveOptionalAsync(user);
    }

    public async Task<IUser?> GetAsync(ClaimsPrincipal principal,
        CancellationToken ct = default)
    {
        Guard.NotNull(principal);

        var user = await userManager.GetUserAsync(principal);

        return await ResolveOptionalAsync(user);
    }

    public async Task<IUser?> FindByIdAsync(string id,
        CancellationToken ct = default)
    {
        if (!userFactory.IsId(id))
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(id);

        return await ResolveOptionalAsync(user);
    }

    public async Task<IUser> CreateAsync(string email, UserValues? values = null, bool lockAutomatically = false,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(email);

        var user = userFactory.Create(email);
        try
        {
            var isFirst = !userManager.Users.Any();

            await userManager.CreateAsync(user).Throw(log);

            values ??= new UserValues();
            values.Roles ??= [];

            if (string.IsNullOrWhiteSpace(values.DisplayName))
            {
                values.DisplayName = email;
            }

            if (isFirst)
            {
                values.Roles.Add(NotifoRoles.HostAdmin);
            }

            await userManager.SyncClaims(user, values).Throw(log);

            if (!string.IsNullOrWhiteSpace(values.Password))
            {
                await userManager.AddPasswordAsync(user, values.Password).Throw(log);
            }

            foreach (var role in values.Roles)
            {
                await userManager.AddToRoleAsync(user, role).Throw(log);
            }

            if (!isFirst && lockAutomatically)
            {
                await userManager.SetLockoutEndDateAsync(user, LockoutDate()).Throw(log);
            }
        }
        catch (Exception)
        {
            try
            {
                if (userFactory.IsId(user.Id))
                {
                    await userManager.DeleteAsync(user);
                }
            }
            catch (Exception ex2)
            {
                log.LogError(ex2, "Failed to cleanup user after creation failed.");
            }

            throw;
        }

        var resolved = await ResolveAsync(user);

        await mediator.PublishAsync(new UserRegistered { User = resolved }, default);

        if (HasConsentGiven(values, null!))
        {
            await mediator.PublishAsync(new UserConsentGiven { User = resolved }, default);
        }

        return resolved;
    }

    public Task<IUser> SetPasswordAsync(string id, string password, string? oldPassword = null,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        return ForUserAsync(id, async user =>
        {
            if (await userManager.HasPasswordAsync(user))
            {
                await userManager.ChangePasswordAsync(user, oldPassword!, password).Throw(log);
            }
            else
            {
                await userManager.AddPasswordAsync(user, password).Throw(log);
            }
        });
    }

    public async Task<IUser> UpdateAsync(string id, UserValues values, bool silent = false,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);
        Guard.NotNull(values);

        var user = await GetUserAsync(id);

        if (!string.IsNullOrWhiteSpace(values.Email) && values.Email != user.Email)
        {
            await userManager.SetEmailAsync(user, values.Email).Throw(log);
            await userManager.SetUserNameAsync(user, values.Email).Throw(log);
        }

        await userManager.SyncClaims(user, values).Throw(log);

        if (!string.IsNullOrWhiteSpace(values.Password))
        {
            if (await userManager.HasPasswordAsync(user))
            {
                await userManager.RemovePasswordAsync(user).Throw(log);
            }

            await userManager.AddPasswordAsync(user, values.Password).Throw(log);
        }

        var oldUser = await ResolveAsync(user);

        if (values.Roles != null)
        {
            foreach (var role in values.Roles.Where(x => !oldUser.Roles.Contains(x)))
            {
                await userManager.AddToRoleAsync(user, role).Throw(log);
            }

            foreach (var role in oldUser.Roles.Where(x => !values.Roles.Contains(x)))
            {
                await userManager.RemoveFromRoleAsync(user, role).Throw(log);
            }
        }

        var resolved = await ResolveAsync(user);

        if (!silent)
        {
            await mediator.PublishAsync(new UserUpdated { User = resolved, OldUser = oldUser }, default);

            if (HasConsentGiven(values, oldUser))
            {
                await mediator.PublishAsync(new UserConsentGiven { User = resolved }, default);
            }
        }

        return resolved;
    }

    public Task<IUser> LockAsync(string id,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        return ForUserAsync(id, user => userManager.SetLockoutEndDateAsync(user, LockoutDate()).Throw(log));
    }

    public Task<IUser> UnlockAsync(string id,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        return ForUserAsync(id, user => userManager.SetLockoutEndDateAsync(user, null).Throw(log));
    }

    public Task<IUser> AddLoginAsync(string id, ExternalLoginInfo externalLogin,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        return ForUserAsync(id, user => userManager.AddLoginAsync(user, externalLogin).Throw(log));
    }

    public Task<IUser> RemoveLoginAsync(string id, string loginProvider, string providerKey,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        return ForUserAsync(id, user => userManager.RemoveLoginAsync(user, loginProvider, providerKey).Throw(log));
    }

    public async Task DeleteAsync(string id, bool silent = false,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        var user = await GetUserAsync(id);

        await userManager.DeleteAsync(user).Throw(log);

        if (!silent)
        {
            var resolved = await ResolveAsync(user);

            await mediator.PublishAsync(new UserDeleted { User = resolved }, default);
        }
    }

    private async Task<IUser> ForUserAsync(string id, Func<IdentityUser, Task> action)
    {
        var user = await GetUserAsync(id);

        await action(user);

        return await ResolveAsync(user);
    }

    private async Task<IdentityUser> GetUserAsync(string id)
    {
        if (!userFactory.IsId(id))
        {
            throw new DomainObjectNotFoundException(id);
        }

        var user = await userManager.FindByIdAsync(id);

        return user ?? throw new DomainObjectNotFoundException(id);
    }

    private Task<IUser[]> ResolveAsync(IEnumerable<IdentityUser> users)
    {
        return Task.WhenAll(users.Select(async user =>
        {
            return await ResolveAsync(user);
        }));
    }

    private async Task<IUser> ResolveAsync(IdentityUser user)
    {
        var (claims, roles, logins, hasPassword) = await AsyncHelper.WhenAll(
            userManager.GetClaimsAsync(user),
            userManager.GetRolesAsync(user),
            userManager.GetLoginsAsync(user),
            userManager.HasPasswordAsync(user));

        return new UserWithClaims(user, claims.ToList(), roles.ToHashSet(), logins.Any() || hasPassword);
    }

    private async Task<IUser?> ResolveOptionalAsync(IdentityUser? user)
    {
        if (user == null)
        {
            return null;
        }

        return await ResolveAsync(user);
    }

    private static bool HasConsentGiven(UserValues values, IUser? oldUser)
    {
        if (values.Consent == true && oldUser?.Claims.HasConsent() != true)
        {
            return true;
        }

        return values.ConsentForEmails == true && oldUser?.Claims.HasConsentForEmails() != true;
    }

    private static DateTimeOffset LockoutDate()
    {
        return DateTimeOffset.UtcNow.AddYears(100);
    }
}
