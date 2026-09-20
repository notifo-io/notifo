// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Migrations;
using Notifo.SqlProviders.SqlServer;
using Notifo.SqlProviders.SqlServer.App;
using PhenX.EntityFrameworkCore.BulkInsert.SqlServer;
using Testcontainers.MsSql;

namespace Notifo.EntityFramework.TestHelpers;

public class SqlServerFixture(string? reuseId) : SqlFixtureBase<SqlServerAppDbContext, MsSqlContainer>
{
    protected override MsSqlContainer Container { get; } =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
            .WithReuse(false)
            .WithLabel("reuse-id", reuseId)
            .Build();

    protected override async Task<string> GetConnectionStringAsync()
    {
        await Container.ExecScriptAsync("create database notifo;");

        var builder = new SqlConnectionStringBuilder(Container.GetConnectionString())
        {
            InitialCatalog = "notifo",
        };

        return builder.ConnectionString;
    }

    protected override void ConfigureServices(IServiceCollection services, string connectionString)
    {
        services
            .AddPooledDbContextFactory<SqlServerAppDbContext>(builder =>
            {
                builder.SetDefaults();
                builder.UseBulkInsertSqlServer();
                builder.UseSqlServer(connectionString);
            })
            .AddSingleton<ConnectionStringParser, SqlServerConnectionStringParser>();
    }
}
