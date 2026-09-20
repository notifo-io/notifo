// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Migrations;
using Npgsql;

namespace Notifo.SqlProviders.Postgres;

public sealed class PostgresConnectionStringParser : ConnectionStringParser
{
    protected override string? GetProviderSpecificHostName(string source)
    {
        var builder = new NpgsqlConnectionStringBuilder(source);

        return builder.Host;
    }
}
