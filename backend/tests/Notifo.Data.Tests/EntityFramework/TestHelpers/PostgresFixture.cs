// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Migrations;
using Notifo.SqlProviders.Postgres;
using Notifo.SqlProviders.Postgres.App;
using PhenX.EntityFrameworkCore.BulkInsert.PostgreSql;
using Testcontainers.PostgreSql;

namespace Notifo.EntityFramework.TestHelpers;

public class PostgresFixture(string? reuseId) : SqlFixtureBase<PostgresAppDbContext, PostgreSqlContainer>
{
    protected override PostgreSqlContainer Container { get; } =
        new PostgreSqlBuilder("postgres:16")
            .WithReuse(false)
            .WithLabel("reuse-id", reuseId)
            .Build();

    protected override void ConfigureServices(IServiceCollection services, string connectionString)
    {
        services
            .AddPooledDbContextFactory<PostgresAppDbContext>(builder =>
            {
                builder.SetDefaults();
                builder.UseBulkInsertPostgreSql();
                builder.UseNpgsql(connectionString);
            })
            .AddSingleton<ConnectionStringParser, PostgresConnectionStringParser>();
    }
}
