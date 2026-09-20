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
using Notifo.SqlProviders.MySql;
using Notifo.SqlProviders.MySql.App;
using PhenX.EntityFrameworkCore.BulkInsert.MySql;
using Testcontainers.MySql;

namespace Notifo.EntityFramework.TestHelpers;

public class MySqlFixture(string? reuseId) : SqlFixtureBase<MySqlAppDbContext, MySqlContainer>
{
    protected override MySqlContainer Container { get; } =
        new MySqlBuilder("mysql:8.0")
            .WithReuse(false)
            .WithLabel("reuse-id", reuseId)
            .WithCommand("--local-infile=1")
            .Build();

    protected override Task<string> GetConnectionStringAsync()
    {
        // Bulk inserts need the local infile feature.
        return Task.FromResult($"{Container.GetConnectionString()};AllowLoadLocalInfile=true");
    }

    protected override void ConfigureServices(IServiceCollection services, string connectionString)
    {
        var version = ServerVersion.AutoDetect(connectionString);

        services
            .AddPooledDbContextFactory<MySqlAppDbContext>(builder =>
            {
                builder.SetDefaults();
                builder.UseBulkInsertMySql();
                builder.UseMySql(connectionString, version);
            })
            .AddSingleton<ConnectionStringParser, MySqlConnectionStringParser>();
    }
}
