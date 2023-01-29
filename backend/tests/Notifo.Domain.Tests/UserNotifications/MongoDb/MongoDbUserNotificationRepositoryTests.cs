// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using NodaTime;
using Notifo.Domain.Channels;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Notifo.Domain.UserNotifications.MongoDb;

[Trait("Category", "Dependencies")]
public class MongoDbUserNotificationRepositoryTests : IClassFixture<MongoDbUserNotificationRepositoryFixture>
{
    private readonly Guid configurationId1 = Guid.NewGuid();
    private readonly Guid configurationId2 = Guid.NewGuid();
    private readonly Instant now = Instant.FromUtc(2022, 11, 10, 9, 8, 7);
    private readonly string appId = "my-app";
    private readonly string channel = "webpush";
    private readonly string configuration1 = Guid.NewGuid().ToString();
    private readonly string configuration2 = Guid.NewGuid().ToString();
    private readonly string userId1 = Guid.NewGuid().ToString();
    private readonly string userId2 = Guid.NewGuid().ToString();

    public MongoDbUserNotificationRepositoryFixture _ { get; }

    public MongoDbUserNotificationRepositoryTests(MongoDbUserNotificationRepositoryFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_store_notification()
    {
        var notification1 = CreateNotification(userId1);
        var notification2 = CreateNotification(userId2);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.InsertAsync(notification2, default);

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);
        var notifications2 = await _.Repository.QueryAsync(appId, userId2, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
        notifications2.ToArray().Should().BeEquivalentTo(new[] { notification2 });
    }

    [Fact]
    public async Task Should_cleanup_old_notifications()
    {
        var time = now;

        for (var i = 0; i < 200; i++)
        {
            await _.Repository.InsertAsync(CreateNotification(userId1, time), default);

            time = time.Plus(Duration.FromSeconds(1));
        }

        var notifications = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery { TotalNeeded = true }, default);

        Assert.Equal(100, notifications.Total);
    }

    [Fact]
    public async Task Should_update_status()
    {
        var notification1 = CreateNotification(userId1);
        var notification2 = CreateNotification(userId2);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.InsertAsync(notification2, default);

        var result = new DeliveryResult(DeliveryStatus.Handled, "Update Details");

        await _.Repository.BatchWriteAsync(new (TrackingToken Token, DeliveryResult Result)[]
        {
            (new TrackingToken(notification1.Id, channel, configurationId1), result),
            (new TrackingToken(notification1.Id, channel, configurationId2), result),
            (new TrackingToken(notification2.Id, channel, configurationId1), result),
            (new TrackingToken(notification2.Id, channel, configurationId2), result),
        }, now, default);

        UpdateStatus(notification1, channel, configurationId1, result);
        UpdateStatus(notification1, channel, configurationId2, result);
        UpdateStatus(notification2, channel, configurationId1, result);
        UpdateStatus(notification2, channel, configurationId2, result);

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);
        var notifications2 = await _.Repository.QueryAsync(appId, userId2, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
        notifications2.ToArray().Should().BeEquivalentTo(new[] { notification2 });
    }

    [Fact]
    public async Task Should_update_status_with_configuration_string()
    {
        var notification1 = CreateNotification(userId1);
        var notification2 = CreateNotification(userId2);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.InsertAsync(notification2, default);

        var result = new DeliveryResult(DeliveryStatus.Handled, "Update Details");

        await _.Repository.BatchWriteAsync(new (TrackingToken Token, DeliveryResult Result)[]
        {
            (new TrackingToken(notification1.Id, channel, default, configuration1), result),
            (new TrackingToken(notification1.Id, channel, default, configuration2), result),
            (new TrackingToken(notification2.Id, channel, default, configuration1), result),
            (new TrackingToken(notification2.Id, channel, default, configuration2), result),
        }, now, default);

        UpdateStatus(notification1, channel, configurationId1, result);
        UpdateStatus(notification1, channel, configurationId2, result);
        UpdateStatus(notification2, channel, configurationId1, result);
        UpdateStatus(notification2, channel, configurationId2, result);

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);
        var notifications2 = await _.Repository.QueryAsync(appId, userId2, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
        notifications2.ToArray().Should().BeEquivalentTo(new[] { notification2 });
    }

    [Fact]
    public async Task Should_not_update_status_without_channel()
    {
        var notification1 = CreateNotification(userId1);
        var notification2 = CreateNotification(userId2);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.InsertAsync(notification2, default);

        var result = new DeliveryResult(DeliveryStatus.Handled, "Update Details");

        await _.Repository.BatchWriteAsync(new (TrackingToken Token, DeliveryResult Result)[]
        {
            (new TrackingToken(notification1.Id), result),
            (new TrackingToken(notification2.Id), result),
        }, now, default);

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);
        var notifications2 = await _.Repository.QueryAsync(appId, userId2, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
        notifications2.ToArray().Should().BeEquivalentTo(new[] { notification2 });
    }

    [Fact]
    public async Task Should_not_update_status_without_configuration()
    {
        var notification1 = CreateNotification(userId1);
        var notification2 = CreateNotification(userId2);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.InsertAsync(notification2, default);

        var result = new DeliveryResult(DeliveryStatus.Handled, "Update Details");

        await _.Repository.BatchWriteAsync(new (TrackingToken Token, DeliveryResult Result)[]
        {
            (new TrackingToken(notification1.Id, channel), result),
            (new TrackingToken(notification2.Id, channel), result),
        }, now, default);

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);
        var notifications2 = await _.Repository.QueryAsync(appId, userId2, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
        notifications2.ToArray().Should().BeEquivalentTo(new[] { notification2 });
    }

    [Fact]
    public async Task Should_mark_as_delivered()
    {
        var notification = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackDeliveredAsync(new[] { new TrackingToken(notification.Id, channel, configurationId1) }, now, default);

        var info = new HandledInfo(now, channel);

        notification.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification.Channels[channel].FirstDelivered = now;
        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_delivered_with_old_format()
    {
        var notification = CreateNotification(userId1);

        await InsertOldRepresentation(notification);

        await _.Repository.TrackDeliveredAsync(new[] { new TrackingToken(notification.Id, channel, default, configuration1) }, now, default);

        var result = (await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default)).Single();

        Assert.Contains(result.Channels[channel].Status, x => x.Value.FirstDelivered == now);
    }

    [Fact]
    public async Task Should_mark_as_delivered_with_configuration()
    {
        var notification = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackDeliveredAsync(new[] { new TrackingToken(notification.Id, channel, default, configuration1) }, now, default);

        var info = new HandledInfo(now, channel);

        notification.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification.Channels[channel].FirstDelivered = now;
        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_delivered_without_configuration_id()
    {
        var notification = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackDeliveredAsync(new[] { new TrackingToken(notification.Id, channel) }, now, default);

        var info = new HandledInfo(now, channel);

        notification.Channels[channel].FirstDelivered = now;
        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_delivered_without_channel()
    {
        var notification = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackDeliveredAsync(new[] { new TrackingToken(notification.Id) }, now, default);

        var info = new HandledInfo(now, null);

        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_seen()
    {
        var notification = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackSeenAsync(new[] { new TrackingToken(notification.Id, channel, configurationId1) }, now, default);

        var info = new HandledInfo(now, channel);

        notification.Channels[channel].Status[configurationId1].FirstSeen = now;
        notification.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification.Channels[channel].FirstSeen = now;
        notification.Channels[channel].FirstDelivered = now;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_seen_with_old_format()
    {
        var notification = CreateNotification(userId1);

        await InsertOldRepresentation(notification);

        await _.Repository.TrackSeenAsync(new[] { new TrackingToken(notification.Id, channel, default, configuration1) }, now, default);

        var result = (await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default)).Single();

        Assert.Contains(result.Channels[channel].Status, x => x.Value.FirstSeen == now);
        Assert.Contains(result.Channels[channel].Status, x => x.Value.FirstDelivered == now);
    }

    [Fact]
    public async Task Should_mark_as_seen_with_configuration()
    {
        var notification = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackSeenAsync(new[] { new TrackingToken(notification.Id, channel, default, configuration1) }, now, default);

        var info = new HandledInfo(now, channel);

        notification.Channels[channel].Status[configurationId1].FirstSeen = now;
        notification.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification.Channels[channel].FirstSeen = now;
        notification.Channels[channel].FirstDelivered = now;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_seen_without_configuration_id()
    {
        var notification = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackSeenAsync(new[] { new TrackingToken(notification.Id, channel) }, now, default);

        var info = new HandledInfo(now, channel);

        notification.Channels[channel].FirstSeen = now;
        notification.Channels[channel].FirstDelivered = now;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_seen_without_channel()
    {
        var notification = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackSeenAsync(new[] { new TrackingToken(notification.Id) }, now, default);

        var info = new HandledInfo(now, null);

        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_not_mark_as_confirmed_if_confirm_mode_not_set()
    {
        var notification = CreateNotification(userId1);

        notification.Formatting.ConfirmMode = ConfirmMode.None;

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackConfirmedAsync(new[] { new TrackingToken(notification.Id, channel, configurationId1) }, now, default);

        var info = new HandledInfo(now, channel);

        notification.Channels[channel].Status[configurationId1].FirstSeen = now;
        notification.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification.Channels[channel].FirstSeen = now;
        notification.Channels[channel].FirstDelivered = now;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_confirmed()
    {
        var notification = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackConfirmedAsync(new[] { new TrackingToken(notification.Id, channel, configurationId1) }, now, default);

        var info = new HandledInfo(now, channel);

        notification.Updated = now;
        notification.Channels[channel].Status[configurationId1].FirstConfirmed = now;
        notification.Channels[channel].Status[configurationId1].FirstSeen = now;
        notification.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification.Channels[channel].FirstConfirmed = now;
        notification.Channels[channel].FirstSeen = now;
        notification.Channels[channel].FirstDelivered = now;
        notification.FirstConfirmed = info;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_confirmed_with_old_format()
    {
        var notification = CreateNotification(userId1);

        await InsertOldRepresentation(notification);

        await _.Repository.TrackConfirmedAsync(new[] { new TrackingToken(notification.Id, channel, default, configuration1) }, now, default);

        var result = (await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default)).Single();

        Assert.Contains(result.Channels[channel].Status, x => x.Value.FirstConfirmed == now);
        Assert.Contains(result.Channels[channel].Status, x => x.Value.FirstSeen == now);
        Assert.Contains(result.Channels[channel].Status, x => x.Value.FirstDelivered == now);
    }

    [Fact]
    public async Task Should_mark_as_confirmed_with_configuration()
    {
        var notification = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackConfirmedAsync(new[] { new TrackingToken(notification.Id, channel, default, configuration1) }, now, default);

        var info = new HandledInfo(now, channel);

        notification.Updated = now;
        notification.Channels[channel].Status[configurationId1].FirstConfirmed = now;
        notification.Channels[channel].Status[configurationId1].FirstSeen = now;
        notification.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification.Channels[channel].FirstConfirmed = now;
        notification.Channels[channel].FirstSeen = now;
        notification.Channels[channel].FirstDelivered = now;
        notification.FirstConfirmed = info;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_confirmed_without_configuration_id()
    {
        var notification = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackConfirmedAsync(new[] { new TrackingToken(notification.Id, channel) }, now, default);

        var info = new HandledInfo(now, channel);

        notification.Updated = now;
        notification.Channels[channel].FirstConfirmed = now;
        notification.Channels[channel].FirstSeen = now;
        notification.Channels[channel].FirstDelivered = now;
        notification.FirstConfirmed = info;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_confirmed_without_channel()
    {
        var notification = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification, default);
        await _.Repository.TrackConfirmedAsync(new[] { new TrackingToken(notification.Id) }, now, default);

        var info = new HandledInfo(now, null);

        notification.Updated = now;
        notification.FirstConfirmed = info;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    private void UpdateStatus(UserNotification notification, string channel, Guid configurationId, DeliveryResult result)
    {
        var statusItem = notification.Channels[channel].Status[configurationId];

        statusItem.LastUpdate = now;
        statusItem.Status = result.Status;
        statusItem.Detail = result.Detail;
    }

    private async Task InsertOldRepresentation(UserNotification notification)
    {
        var bsonDocument = notification.ToBsonDocument();

        var oldStatus = new Dictionary<string, ChannelSendInfo>
        {
            [configuration1.ToBase64()] = new ChannelSendInfo(),
            [configuration2.ToBase64()] = new ChannelSendInfo(),
        }.ToBsonDocument();

        foreach (var element in bsonDocument["Channels"].AsBsonDocument)
        {
            element.Value.AsBsonDocument["Status"] = oldStatus;
        }

        var collection = _.MongoDatabase.GetCollection<BsonDocument>(_.Repository.Collection.CollectionNamespace.CollectionName);

        await collection.InsertOneAsync(bsonDocument);
    }

    private UserNotification CreateNotification(string userId, Instant created = default)
    {
        return new UserNotification
        {
            Id = Guid.NewGuid(),
            AppId = appId,
            Channels = new Dictionary<string, UserNotificationChannel>
            {
                [channel] = new UserNotificationChannel
                {
                    Setting = new ChannelSetting
                    {
                        Send = ChannelSend.Send
                    },
                    Status = new Dictionary<Guid, ChannelSendInfo>
                    {
                        [configurationId1] = new ChannelSendInfo
                        {
                            Configuration = new SendConfiguration
                            {
                                ["key1"] = configuration1
                            }
                        },
                        [configurationId2] = new ChannelSendInfo
                        {
                            Configuration = new SendConfiguration
                            {
                                ["key2"] = configuration2
                            }
                        }
                    }
                }
            },
            Formatting = new NotificationFormatting<string>()
            {
                ConfirmMode = ConfirmMode.Explicit
            },
            UserId = userId,
            UserLanguage = "en",
            Created = created
        };
    }
}
