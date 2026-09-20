// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Infrastructure.KeyValueStore;
using Notifo.Shared;

namespace Notifo.EntityFramework.Infrastructure;

public abstract class EFKeyValueStoreTests<TContext>(ISqlFixture<TContext> fixture) : KeyValueStoreTests where TContext : DbContext
{
    protected override Task<IKeyValueStore> CreateSutAsync()
    {
        var sut = new EFKeyValueStore<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IKeyValueStore>(sut);
    }
}
