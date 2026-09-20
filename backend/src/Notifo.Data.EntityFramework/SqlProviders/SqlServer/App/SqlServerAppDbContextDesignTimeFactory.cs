// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notifo.SqlProviders.SqlServer.App;

public sealed class SqlServerAppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<SqlServerAppDbContext>
{
    public SqlServerAppDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost,14330;Database=test;User=sa;Password=SqlServer2026!?;TrustServerCertificate=True";

        var builder = new DbContextOptionsBuilder<SqlServerAppDbContext>()
            .UseSqlServer(ConnectionString);

        return new SqlServerAppDbContext(builder.Options, JsonSerializerOptions.Default);
    }
}
