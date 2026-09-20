// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Infrastructure.Migrations;

namespace Notifo.EntityFramework.Migrations;

public abstract class EFMigrationTests<TContext>(ISqlFixture<TContext> fixture) where TContext : DbContext
{
    [Fact]
    public async Task Should_apply_all_migrations()
    {
        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();

        var applied = await dbContext.Database.GetAppliedMigrationsAsync();
        var pending = await dbContext.Database.GetPendingMigrationsAsync();

        Assert.NotEmpty(applied);
        Assert.Empty(pending);
    }

    [Fact]
    public async Task Should_migrate_idempotent()
    {
        var migrator = fixture.Services.GetRequiredService<DatabaseMigrator<TContext>>();

        await migrator.InitializeAsync(default);
        await migrator.InitializeAsync(default);
    }

    [Fact]
    public async Task Should_not_have_model_changes_without_migration()
    {
        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();

        // Fails when the model has been changed, but no migration has been created with the migration script.
        Assert.False(dbContext.Database.HasPendingModelChanges());
    }
}
