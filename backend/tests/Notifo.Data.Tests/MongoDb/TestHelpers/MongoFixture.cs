// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;
using Testcontainers.MongoDb;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Infrastructure.Fixtures;

[CollectionDefinition(Name)]
public sealed class MongoFixtureCollection : ICollectionFixture<MongoFixture>
{
    public const string Name = "Mongo";
}

public class MongoFixture : IAsyncLifetime
{
    private readonly MongoDbContainer mongoDb =
        new MongoDbBuilder("mongo:6.0")
            .WithReuse(false)
            .WithLabel("reuse-id", "notifo-mongodb")
            .Build();

    public IMongoClient Client { get; private set; }

    public IMongoDatabase Database => Client.GetDatabase("Test");

    public async Task InitializeAsync()
    {
        await mongoDb.StartAsync();

        Client = MongoClientFactory.Create(mongoDb.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await mongoDb.StopAsync();
    }
}
