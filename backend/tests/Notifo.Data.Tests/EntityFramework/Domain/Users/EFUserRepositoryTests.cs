// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Users;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Shared;

namespace Notifo.EntityFramework.Domain.Users;

public abstract class EFUserRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : UserRepositoryTests where TContext : DbContext
{
    protected override Task<IUserRepository> CreateSutAsync()
    {
        var sut = new EFUserRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IUserRepository>(sut);
    }
}
