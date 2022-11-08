// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions;
using NodaTime;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Notifo.Domain.UserNotifications.MongoDb
{
    [Trait("Category", "Dependencies")]
    public class MongoDbUserNotificationRepositoryTests : IClassFixture<MongoDbUserNotificationRepositoryFixture>
    {
        private readonly string appId = "my-app";
        private readonly string userId1 = Guid.NewGuid().ToString();
        private readonly string userId2 = Guid.NewGuid().ToString();
        private readonly string channel = "webpush";
        private readonly Guid token1 = Guid.NewGuid();
        private readonly Guid token2 = Guid.NewGuid();

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
            var now = SystemClock.Instance.GetCurrentInstant();

            for (var i = 0; i < 200; i++)
            {
                await _.Repository.InsertAsync(CreateNotification(userId1, now), default);

                now = now.Plus(Duration.FromSeconds(1));
            }

            var notifications = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery { TotalNeeded = true }, default);

            Assert.Equal(100, notifications.Total);
        }

        [Fact]
        public async Task Should_update_notifications()
        {
            var notification1 = CreateNotification(userId1);
            var notification2 = CreateNotification(userId2);

            await _.Repository.InsertAsync(notification1, default);
            await _.Repository.InsertAsync(notification2, default);

            var update = new ChannelSendInfo
            {
                LastUpdate = Instant.FromUtc(2020, 12, 11, 10, 9, 8),
                Detail = "Something",
                Status = ProcessStatus.Failed
            };

            await _.Repository.BatchWriteAsync(new[]
            {
                (notification1.Id, channel, token1, update),
                (notification1.Id, channel, token2, update),
                (notification2.Id, channel, token1, update),
                (notification2.Id, channel, token2, update)
            }, default);

            notification1.Channels[channel].Status[token1] = update;
            notification1.Channels[channel].Status[token2] = update;

            notification2.Channels[channel].Status[token1] = update;
            notification2.Channels[channel].Status[token2] = update;

            var notifications1 = await _.Repository.QueryAsync(appId, userId1, new UserNotificationQuery(), default);
            var notifications2 = await _.Repository.QueryAsync(appId, userId2, new UserNotificationQuery(), default);

            notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
            notifications2.ToArray().Should().BeEquivalentTo(new[] { notification2 });
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
                            [token1] = new ChannelSendInfo(),
                            [token2] = new ChannelSendInfo()
                        }
                    }
                },
                Formatting = new NotificationFormatting<string>(),
                UserId = userId,
                UserLanguage = "en",
                Created = created
            };
        }
    }
}
