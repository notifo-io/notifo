// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Subscriptions;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Shared;

namespace Notifo.EntityFramework.Domain.Subscriptions;

public abstract class EFSubscriptionRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : SubscriptionRepositoryTests where TContext : DbContext
{
    protected override Task<ISubscriptionRepository> CreateSutAsync()
    {
        var sut = new EFSubscriptionRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<ISubscriptionRepository>(sut);
    }
}
