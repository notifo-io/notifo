// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Notifo.SqlProviders.MySql.App;

public sealed class MySqlAppDbContext(DbContextOptions options, JsonSerializerOptions jsonOptions)
    : AppDbContext(options, jsonOptions)
{
}
