// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Log;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Shared;

namespace Notifo.EntityFramework.Domain.Log;

public abstract class EFLogRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : LogRepositoryTests where TContext : DbContext
{
    protected override Task<ILogRepository> CreateSutAsync()
    {
        var sut = new EFLogRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<ILogRepository>(sut);
    }
}
