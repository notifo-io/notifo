﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Notifo.Infrastructure.Timers;
using OpenIddict.Abstractions;
using OpenIddict.MongoDb;
using OpenIddict.MongoDb.Models;
using Squidex.Hosting;

namespace Notifo.Identity;

public sealed class TokenStoreInitializer(
    IOptions<OpenIddictMongoDbOptions> options,
    IServiceProvider serviceProvider)
    : IInitializable
{
    private readonly OpenIddictMongoDbOptions options = options.Value;
    private CompletionTimer timer;

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        await SetupIndexAsync(ct);

        timer = new CompletionTimer((int)TimeSpan.FromHours(6).TotalMilliseconds, PruneAsync);
    }

    public Task ReleaseAsync(
        CancellationToken ct)
    {
        return timer?.StopAsync() ?? Task.CompletedTask;
    }

    private async Task PruneAsync(
        CancellationToken ct)
    {
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var tokenManager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();

            await tokenManager.PruneAsync(DateTimeOffset.UtcNow.AddDays(-40), ct);
        }
    }

    private async Task SetupIndexAsync(
        CancellationToken ct)
    {
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var database = await scope.ServiceProvider.GetRequiredService<IOpenIddictMongoDbContext>().GetDatabaseAsync(ct);

            var collection = database.GetCollection<OpenIddictMongoDbToken>(options.TokensCollectionName);

            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<OpenIddictMongoDbToken>(
                    Builders<OpenIddictMongoDbToken>.IndexKeys
                        .Ascending(x => x.ReferenceId)),
                cancellationToken: ct);
        }
    }
}
