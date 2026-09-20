// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Hosting;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Notifo.Infrastructure.Migrations;

public sealed class DatabaseMigrator<TContext>(IDbContextFactory<TContext> dbContextFactory, ConnectionStringParser parser) : IInitializable
    where TContext : DbContext
{
    private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(30);

    public int Order => -1000;

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(WaitTime);

            while (!await dbContext.Database.CanConnectAsync(cts.Token))
            {
                await Task.Delay(100, cts.Token);
            }
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            var connectionString = dbContext.Database.GetConnectionString();

            var hostName = parser.GetHostName(connectionString);

            if (string.IsNullOrWhiteSpace(hostName))
            {
                hostName = "Unknown";
            }

            throw new InvalidOperationException($"Failed to connect to database <{hostName}>");
        }

        await dbContext.Database.MigrateAsync(ct);
    }
}
