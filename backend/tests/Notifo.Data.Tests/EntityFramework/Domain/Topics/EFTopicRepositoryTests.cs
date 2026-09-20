// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Topics;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Shared;

namespace Notifo.EntityFramework.Domain.Topics;

public abstract class EFTopicRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : TopicRepositoryTests where TContext : DbContext
{
    protected override Task<ITopicRepository> CreateSutAsync()
    {
        var sut = new EFTopicRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<ITopicRepository>(sut);
    }
}
