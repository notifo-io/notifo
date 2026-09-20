// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Notifo.SqlProviders.Postgres.App;

public sealed class PostgresAppDbContext(DbContextOptions options, JsonSerializerOptions jsonOptions)
    : AppDbContext(options, jsonOptions)
{
}
