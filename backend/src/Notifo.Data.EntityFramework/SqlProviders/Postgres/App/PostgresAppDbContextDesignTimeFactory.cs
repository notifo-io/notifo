// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notifo.SqlProviders.Postgres.App;

public sealed class PostgresAppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<PostgresAppDbContext>
{
    public PostgresAppDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost;Port=54320;Database=test;Username=postgres;Password=postgres";

        var builder = new DbContextOptionsBuilder<PostgresAppDbContext>()
            .UseNpgsql(ConnectionString);

        return new PostgresAppDbContext(builder.Options, JsonSerializerOptions.Default);
    }
}
