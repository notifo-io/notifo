// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Identity;

namespace Notifo.EntityFramework.Identity;

public abstract class EFUserStoreTests<TContext>(ISqlFixture<TContext> fixture) where TContext : DbContext
{
    [Fact]
    public async Task Should_create_role_implicitly_when_added_to_user()
    {
        var roleName = Guid.NewGuid().ToString().ToUpperInvariant();

        var user1 = await CreateUserAsync();
        var user2 = await CreateUserAsync();

        await AddToRoleAsync(user1, roleName);
        await AddToRoleAsync(user2, roleName);

        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();

        var store = new EFUserStore<TContext>(dbContext);

        Assert.Equal([roleName], await store.GetRolesAsync(user1));
        Assert.Equal([roleName], await store.GetRolesAsync(user2));
        Assert.Equal(1, await dbContext.Set<IdentityRole>().CountAsync(x => x.NormalizedName == roleName));
    }

    private async Task<IdentityUser> CreateUserAsync()
    {
        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();

        var store = new EFUserStore<TContext>(dbContext);

        var email = $"{Guid.NewGuid()}@notifo.io";

        var user = new EFUserFactory().Create(email);

        await store.CreateAsync(user);

        return user;
    }

    private async Task AddToRoleAsync(IdentityUser user, string roleName)
    {
        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();

        var store = new EFUserStore<TContext>(dbContext);

        await store.AddToRoleAsync(user, roleName);
        await store.UpdateAsync(user);
    }
}
