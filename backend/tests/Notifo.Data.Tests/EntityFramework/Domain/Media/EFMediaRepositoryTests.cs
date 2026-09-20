// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Media;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Shared;

namespace Notifo.EntityFramework.Domain.Media;

public abstract class EFMediaRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : MediaRepositoryTests where TContext : DbContext
{
    protected override Task<IMediaRepository> CreateSutAsync()
    {
        var sut = new EFMediaRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IMediaRepository>(sut);
    }
}
