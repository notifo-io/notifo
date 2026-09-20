// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;

namespace Notifo.EntityFramework.TestHelpers;

public interface ISqlFixture<TContext> where TContext : DbContext
{
    IDbContextFactory<TContext> DbContextFactory { get; }

    IServiceProvider Services { get; }
}
