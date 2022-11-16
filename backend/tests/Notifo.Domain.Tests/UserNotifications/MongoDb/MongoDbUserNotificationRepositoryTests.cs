// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions;
using Google.Rpc;
using NodaTime;
using Notifo.Domain.Channels;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Notifo.Domain.UserNotifications.MongoDb;

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

        var updateStatus = ProcessStatus.Handled;
        var updateDetail = "Update Details";

        await _.Repository.BatchWriteAsync(new (TrackingToken Token, ProcessStatus Status, string? Detail)[]
        {
            (new TrackingToken(notification1.Id, channel, configurationId1), updateStatus, updateDetail),
            (new TrackingToken(notification1.Id, channel, configurationId2), updateStatus, updateDetail),
            (new TrackingToken(notification2.Id, channel, configurationId1), updateStatus, updateDetail),
            (new TrackingToken(notification2.Id, channel, configurationId2), updateStatus, updateDetail),
        }, now, default);

        UpdateStatus(notification1, channel, configurationId1, updateStatus, updateDetail);
        UpdateStatus(notification1, channel, configurationId2, updateStatus, updateDetail);
        UpdateStatus(notification2, channel, configurationId1, updateStatus, updateDetail);
        UpdateStatus(notification2, channel, configurationId2, updateStatus, updateDetail);

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

        var updateStatus = ProcessStatus.Handled;
        var updateDetail = "Update Details";

        await _.Repository.BatchWriteAsync(new (TrackingToken Token, ProcessStatus Status, string? Detail)[]
        {
            (new TrackingToken(notification1.Id, channel, default, configuration1), updateStatus, updateDetail),
            (new TrackingToken(notification1.Id, channel, default, configuration2), updateStatus, updateDetail),
            (new TrackingToken(notification2.Id, channel, default, configuration1), updateStatus, updateDetail),
            (new TrackingToken(notification2.Id, channel, default, configuration2), updateStatus, updateDetail),
        }, now, default);

        UpdateStatus(notification1, channel, configurationId1, updateStatus, updateDetail);
        UpdateStatus(notification1, channel, configurationId2, updateStatus, updateDetail);
        UpdateStatus(notification2, channel, configurationId1, updateStatus, updateDetail);
        UpdateStatus(notification2, channel, configurationId2, updateStatus, updateDetail);

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

        var updateStatus = ProcessStatus.Handled;

        await _.Repository.BatchWriteAsync(new (TrackingToken Token, ProcessStatus Status, string? Detail)[]
        {
            (new TrackingToken(notification1.Id), updateStatus, null),
            (new TrackingToken(notification2.Id), updateStatus, null),
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

        var updateStatus = ProcessStatus.Handled;

        await _.Repository.BatchWriteAsync(new (TrackingToken Token, ProcessStatus Status, string? Detail)[]
        {
            (new TrackingToken(notification1.Id, channel), updateStatus, null),
            (new TrackingToken(notification2.Id, channel), updateStatus, null),
        }, now, default);

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);
        var notifications2 = await _.Repository.QueryAsync(appId, userId2, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
        notifications2.ToArray().Should().BeEquivalentTo(new[] { notification2 });
    }

    [Fact]
    public async Task Should_mark_as_delivered()
    {
        var notification1 = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackDeliveredAsync(new[] { new TrackingToken(notification1.Id, channel, configurationId1) }, now, default);

        var info = new HandledInfo(now, channel);

        notification1.Updated = now;
        notification1.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification1.Channels[channel].FirstDelivered = now;
        notification1.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    [Fact]
    public async Task Should_mark_as_delivered_with_configuration()
    {
        var notification1 = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackDeliveredAsync(new[] { new TrackingToken(notification1.Id, channel, default, configuration1) }, now, default);

        var info = new HandledInfo(now, channel);

        notification1.Updated = now;
        notification1.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification1.Channels[channel].FirstDelivered = now;
        notification1.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    [Fact]
    public async Task Should_mark_as_delivered_without_configuration_id()
    {
        var notification1 = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackDeliveredAsync(new[] { new TrackingToken(notification1.Id, channel) }, now, default);

        var info = new HandledInfo(now, channel);

        notification1.Updated = now;
        notification1.Channels[channel].FirstDelivered = now;
        notification1.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    [Fact]
    public async Task Should_mark_as_delivered_without_channel()
    {
        var notification1 = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackDeliveredAsync(new[] { new TrackingToken(notification1.Id) }, now, default);

        var info = new HandledInfo(now, null);

        notification1.Updated = now;
        notification1.FirstDelivered = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    [Fact]
    public async Task Should_mark_as_seen()
    {
        var notification1 = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackSeenAsync(new[] { new TrackingToken(notification1.Id, channel, configurationId1) }, now, default);

        var info = new HandledInfo(now, channel);

        notification1.Updated = now;
        notification1.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification1.Channels[channel].Status[configurationId1].FirstSeen = now;
        notification1.Channels[channel].FirstDelivered = now;
        notification1.Channels[channel].FirstSeen = now;
        notification1.FirstDelivered = info;
        notification1.FirstSeen = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    [Fact]
    public async Task Should_mark_as_seen_with_configuration()
    {
        var notification1 = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackSeenAsync(new[] { new TrackingToken(notification1.Id, channel, default, configuration1) }, now, default);

        var info = new HandledInfo(now, channel);

        notification1.Updated = now;
        notification1.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification1.Channels[channel].Status[configurationId1].FirstSeen = now;
        notification1.Channels[channel].FirstDelivered = now;
        notification1.Channels[channel].FirstSeen = now;
        notification1.FirstDelivered = info;
        notification1.FirstSeen = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    [Fact]
    public async Task Should_mark_as_seen_without_configuration_id()
    {
        var notification1 = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackSeenAsync(new[] { new TrackingToken(notification1.Id, channel) }, now, default);

        var info = new HandledInfo(now, channel);

        notification1.Updated = now;
        notification1.Channels[channel].FirstDelivered = now;
        notification1.Channels[channel].FirstSeen = now;
        notification1.FirstDelivered = info;
        notification1.FirstSeen = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    [Fact]
    public async Task Should_mark_as_seen_without_channel()
    {
        var notification1 = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackSeenAsync(new[] { new TrackingToken(notification1.Id) }, now, default);

        var info = new HandledInfo(now, null);

        notification1.Updated = now;
        notification1.FirstDelivered = info;
        notification1.FirstSeen = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    [Fact]
    public async Task Should_not_mark_as_confirmed_if_confirm_mode_not_set()
    {
        var notification1 = CreateNotification(userId1);

        notification1.Formatting.ConfirmMode = ConfirmMode.None;

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackConfirmedAsync(new TrackingToken(notification1.Id, channel, configurationId1), now, default);

        var info = new HandledInfo(now, channel);

        notification1.Updated = now;
        notification1.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification1.Channels[channel].Status[configurationId1].FirstSeen = now;
        notification1.Channels[channel].FirstDelivered = now;
        notification1.Channels[channel].FirstSeen = now;
        notification1.FirstDelivered = info;
        notification1.FirstSeen = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    [Fact]
    public async Task Should_mark_as_confirmed()
    {
        var notification1 = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackConfirmedAsync(new TrackingToken(notification1.Id, channel, configurationId1), now, default);

        var info = new HandledInfo(now, channel);

        notification1.Updated = now;
        notification1.Channels[channel].Status[configurationId1].FirstConfirmed = now;
        notification1.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification1.Channels[channel].Status[configurationId1].FirstSeen = now;
        notification1.Channels[channel].FirstConfirmed = now;
        notification1.Channels[channel].FirstDelivered = now;
        notification1.Channels[channel].FirstSeen = now;
        notification1.FirstConfirmed = info;
        notification1.FirstDelivered = info;
        notification1.FirstSeen = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    [Fact]
    public async Task Should_mark_as_confirmed_with_configuration()
    {
        var notification1 = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackConfirmedAsync(new TrackingToken(notification1.Id, channel, default, configuration1), now, default);

        var info = new HandledInfo(now, channel);

        notification1.Updated = now;
        notification1.Channels[channel].Status[configurationId1].FirstConfirmed = now;
        notification1.Channels[channel].Status[configurationId1].FirstDelivered = now;
        notification1.Channels[channel].Status[configurationId1].FirstSeen = now;
        notification1.Channels[channel].FirstConfirmed = now;
        notification1.Channels[channel].FirstDelivered = now;
        notification1.Channels[channel].FirstSeen = now;
        notification1.FirstConfirmed = info;
        notification1.FirstDelivered = info;
        notification1.FirstSeen = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    [Fact]
    public async Task Should_mark_as_confirmed_without_configuration_id()
    {
        var notification1 = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackConfirmedAsync(new TrackingToken(notification1.Id, channel), now, default);

        var info = new HandledInfo(now, channel);

        notification1.Updated = now;
        notification1.Channels[channel].FirstConfirmed = now;
        notification1.Channels[channel].FirstDelivered = now;
        notification1.Channels[channel].FirstSeen = now;
        notification1.FirstConfirmed = info;
        notification1.FirstDelivered = info;
        notification1.FirstSeen = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    [Fact]
    public async Task Should_mark_as_confirmed_without_channel()
    {
        var notification1 = CreateNotification(userId1);

        await _.Repository.InsertAsync(notification1, default);
        await _.Repository.TrackConfirmedAsync(new TrackingToken(notification1.Id), now, default);

        var info = new HandledInfo(now, null);

        notification1.Updated = now;
        notification1.FirstConfirmed = info;
        notification1.FirstDelivered = info;
        notification1.FirstSeen = info;

        var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
    }

    private void UpdateStatus(UserNotification notification, string channel, Guid configurationId, ProcessStatus status, string? detail)
    {
        var statusItem = notification.Channels[channel].Status[configurationId];

        statusItem.LastUpdate = now;
        statusItem.Status = status;
        statusItem.Detail = detail;

        notification.Updated = now;
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
