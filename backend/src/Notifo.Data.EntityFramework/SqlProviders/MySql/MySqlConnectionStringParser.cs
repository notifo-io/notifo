// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MySqlConnector;
using Notifo.Infrastructure.Migrations;

namespace Notifo.SqlProviders.MySql;

public sealed class MySqlConnectionStringParser : ConnectionStringParser
{
    protected override string? GetProviderSpecificHostName(string source)
    {
        var builder = new MySqlConnectionStringBuilder(source);

        return builder.Server;
    }
}
