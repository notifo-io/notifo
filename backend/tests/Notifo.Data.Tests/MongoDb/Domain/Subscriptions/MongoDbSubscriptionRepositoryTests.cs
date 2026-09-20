// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using MongoDB.Bson;
using Notifo.Domain;
using Notifo.Domain.Subscriptions;
using Notifo.MongoDb.TestHelpers;
using Notifo.Shared;

namespace Notifo.MongoDb.Domain.Subscriptions;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbSubscriptionRepositoryTests(MongoFixture fixture) : SubscriptionRepositoryTests
{
    private readonly string empty = Guid.Empty.ToString();

    protected override async Task<ISubscriptionRepository> CreateSutAsync()
    {
        return await CreateRepositoryAsync();
    }

    [Fact]
    [Trait("Category", "Dependencies")]
    public async Task Should_be_fast()
    {
        var repository = await CreateRepositoryAsync();

        var count = await repository.Collection.CountDocumentsAsync(new BsonDocument());

        const int Count = 1_000_000;

        if (count < Count)
        {
            var inserts = new List<MongoDbSubscription>();

            for (var i = 0; i < Count; i++)
            {
                TopicId randomTopic = $"{Guid.NewGuid()}/{Random.Shared.Next(10000)}/{Random.Shared.Next(10000)}/{Random.Shared.Next(10000)}/{Random.Shared.Next(10000)}/{Random.Shared.Next(10000)}a";

                inserts.Add(new MongoDbSubscription
                {
                    DocId = Guid.NewGuid().ToString(),
                    AppId = empty,
                    TopicArray = randomTopic.GetParts(),
                    TopicPrefix = randomTopic,
                    UserId = empty
                });
            }

            await repository.Collection.InsertManyAsync(inserts);
        }

        var topicToSearch = $"{Guid.NewGuid()}/{Random.Shared.Next(10000)}/{Random.Shared.Next(10000)}a";

        await ToList(repository.QueryAsync(empty, topicToSearch));
        await ToList(repository.QueryAsync(empty, topicToSearch));

        var watch = Stopwatch.StartNew();

        await ToList(repository.QueryAsync(empty, topicToSearch));

        watch.Stop();

        Assert.InRange(watch.ElapsedMilliseconds, 0, 20);
    }

    private async Task<MongoDbSubscriptionRepository> CreateRepositoryAsync()
    {
        var sut = new MongoDbSubscriptionRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
