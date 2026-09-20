// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Data.SqlClient;
using Notifo.Infrastructure.Migrations;

namespace Notifo.SqlProviders.SqlServer;

public sealed class SqlServerConnectionStringParser : ConnectionStringParser
{
    protected override string? GetProviderSpecificHostName(string source)
    {
        var builder = new SqlConnectionStringBuilder(source);

        return builder.DataSource;
    }
}
