// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notifo.SqlProviders.MySql.App;

public sealed class MySqlAppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<MySqlAppDbContext>
{
    public MySqlAppDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost;Port=33060;Database=test;User=mysql;Password=mysql";

        var builder = new DbContextOptionsBuilder<MySqlAppDbContext>()
            .UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString));

        return new MySqlAppDbContext(builder.Options, JsonSerializerOptions.Default);
    }
}
