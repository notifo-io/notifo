// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OpenIddict.MongoDb;
using OpenIddict.MongoDb.Models;
using Squidex.Hosting;

namespace Notifo.Identity;

public sealed class MongoDbTokenStoreInitializer(
    IOptions<OpenIddictMongoDbOptions> options,
    IServiceProvider serviceProvider)
    : IInitializable
{
    private readonly OpenIddictMongoDbOptions options = options.Value;

    public async Task InitializeAsync(
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
