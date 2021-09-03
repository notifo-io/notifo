// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Notifo.Infrastructure.Timers;
using OpenIddict.Abstractions;
using OpenIddict.MongoDb;
using OpenIddict.MongoDb.Models;
using Squidex.Hosting;

namespace Notifo.Identity
{
    public sealed class TokenStoreInitializer : IInitializable
    {
        private readonly OpenIddictMongoDbOptions options;
        private readonly IServiceProvider serviceProvider;
        private CompletionTimer timer;

        public TokenStoreInitializer(IOptions<OpenIddictMongoDbOptions> options,
            IServiceProvider serviceProvider)
        {
            this.options = options.Value;

            this.serviceProvider = serviceProvider;
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            await SetupIndexAsync(ct);

            await PruneAsync(ct);

            timer = new CompletionTimer((int)TimeSpan.FromHours(6).TotalMilliseconds, async ct =>
            {
                await PruneAsync(ct);
            });
        }

        public async Task ReleaseAsync(CancellationToken ct)
        {
            if (timer != null)
            {
                await timer.StopAsync();
            }
        }

        private async Task PruneAsync(CancellationToken ct)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var tokenManager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();

                await tokenManager.PruneAsync(DateTimeOffset.UtcNow.AddDays(-40), ct);
            }
        }

        private async Task SetupIndexAsync(CancellationToken ct)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var database = await scope.ServiceProvider.GetRequiredService<IOpenIddictMongoDbContext>().GetDatabaseAsync(ct);

                var collection = database.GetCollection<OpenIddictMongoDbToken<string>>(options.TokensCollectionName);

                await collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<OpenIddictMongoDbToken<string>>(
                        Builders<OpenIddictMongoDbToken<string>>.IndexKeys
                            .Ascending(x => x.ReferenceId)),
                    cancellationToken: ct);
            }
        }
    }
}
