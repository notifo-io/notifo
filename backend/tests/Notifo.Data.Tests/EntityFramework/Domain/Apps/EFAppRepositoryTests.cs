// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Apps;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Shared;

namespace Notifo.EntityFramework.Domain.Apps;

public abstract class EFAppRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : AppRepositoryTests where TContext : DbContext
{
    protected override Task<IAppRepository> CreateSutAsync()
    {
        var sut = new EFAppRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IAppRepository>(sut);
    }
}
