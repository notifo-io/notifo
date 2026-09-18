// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Notifo.Domain.Shared;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Fixtures;

namespace Notifo.Domain.UserNotifications.MongoDb;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbUserNotificationRepositoryTests(MongoFixture fixture) : UserNotificationRepositoryTests
{
    protected override async Task<IUserNotificationRepository> CreateSutAsync()
    {
        return await CreateRepositoryAsync();
    }

    [Fact]
    public async Task Should_mark_as_delivered_with_old_format()
    {
        var repository = await CreateRepositoryAsync();

        var notification = CreateNotification(UserId1);

        await InsertOldRepresentation(repository, notification);

        await repository.TrackDeliveredAsync([new TrackingToken(notification.Id, Channel, default, Configuration1)], Now, default);

        var result = (await repository.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default)).Single();

        Assert.Contains(result.Channels[Channel].Status, x => x.Value.FirstDelivered == Now);
    }

    [Fact]
    public async Task Should_mark_as_seen_with_old_format()
    {
        var repository = await CreateRepositoryAsync();

        var notification = CreateNotification(UserId1);

        await InsertOldRepresentation(repository, notification);

        await repository.TrackSeenAsync([new TrackingToken(notification.Id, Channel, default, Configuration1)], Now, default);

        var result = (await repository.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default)).Single();

        Assert.Contains(result.Channels[Channel].Status, x => x.Value.FirstSeen == Now);
        Assert.Contains(result.Channels[Channel].Status, x => x.Value.FirstDelivered == Now);
    }

    [Fact]
    public async Task Should_mark_as_confirmed_with_old_format()
    {
        var repository = await CreateRepositoryAsync();

        var notification = CreateNotification(UserId1);

        await InsertOldRepresentation(repository, notification);

        await repository.TrackConfirmedAsync([new TrackingToken(notification.Id, Channel, default, Configuration1)], Now, default);

        var result = (await repository.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default)).Single();

        Assert.Contains(result.Channels[Channel].Status, x => x.Value.FirstConfirmed == Now);
        Assert.Contains(result.Channels[Channel].Status, x => x.Value.FirstSeen == Now);
        Assert.Contains(result.Channels[Channel].Status, x => x.Value.FirstDelivered == Now);
    }

    private async Task InsertOldRepresentation(MongoDbUserNotificationRepository repository, UserNotification notification)
    {
        var bsonDocument = notification.ToBsonDocument();

        var oldStatus = new Dictionary<string, ChannelSendInfo>
        {
            [Configuration1.ToBase64()] = new ChannelSendInfo(),
            [Configuration2.ToBase64()] = new ChannelSendInfo(),
        }.ToBsonDocument();

        foreach (var element in bsonDocument["Channels"].AsBsonDocument)
        {
            element.Value.AsBsonDocument["Status"] = oldStatus;
        }

        var collection = fixture.Database.GetCollection<BsonDocument>(repository.Collection.CollectionNamespace.CollectionName);

        await collection.InsertOneAsync(bsonDocument);
    }

    private async Task<MongoDbUserNotificationRepository> CreateRepositoryAsync()
    {
        var sut =
            new MongoDbUserNotificationRepository(fixture.Database,
                Options.Create(new UserNotificationsOptions { MaxItemsPerUser = 100 }),
                A.Fake<ILogger<MongoDbUserNotificationRepository>>());

        await sut.InitializeAsync(default);
        return sut;
    }
}
