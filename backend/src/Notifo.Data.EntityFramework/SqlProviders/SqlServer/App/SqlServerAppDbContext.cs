// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Notifo.SqlProviders.SqlServer.App;

public sealed class SqlServerAppDbContext(DbContextOptions options, JsonSerializerOptions jsonOptions)
    : AppDbContext(options, jsonOptions)
{
}
