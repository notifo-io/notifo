// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Notifo.Infrastructure.Migrations;

namespace Notifo.EntityFramework.TestHelpers;

public abstract class SqlFixtureBase<TContext, TContainer> : IAsyncLifetime, ISqlFixture<TContext>
    where TContext : AppDbContext
    where TContainer : IContainer
{
    private IServiceProvider services;

    protected abstract TContainer Container { get; }

    public IDbContextFactory<TContext> DbContextFactory => services.GetRequiredService<IDbContextFactory<TContext>>();

    public IServiceProvider Services => services;

    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        var connectionString = await GetConnectionStringAsync();

        var serviceCollection =
            new ServiceCollection()
                .AddLogging()
                .AddSingleton(TestJson.Options)
                .AddSingleton<IClock>(SystemClock.Instance)
                .AddSingleton<DatabaseMigrator<TContext>>();

        ConfigureServices(serviceCollection, connectionString);

        services = serviceCollection.BuildServiceProvider();

        // Use the migrations to create the schema, so that we also test them.
        await services.GetRequiredService<DatabaseMigrator<TContext>>().InitializeAsync(default);
    }

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
    }

    protected virtual Task<string> GetConnectionStringAsync()
    {
        return Task.FromResult(((IDatabaseContainer)Container).GetConnectionString());
    }

    protected abstract void ConfigureServices(IServiceCollection services, string connectionString);
}
