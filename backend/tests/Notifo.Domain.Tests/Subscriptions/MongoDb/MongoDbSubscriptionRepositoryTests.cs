// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Notifo.Domain.Channels;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Notifo.Domain.Subscriptions.MongoDb
{
    [Trait("Category", "Dependencies")]
    public class MongoDbSubscriptionRepositoryTests : IClassFixture<MongoDbSubscriptionRepositoryFixture>
    {
        private readonly string topic = Guid.NewGuid().ToString();
        private readonly string appId = "my-app";
        private readonly string userId1 = Guid.NewGuid().ToString();
        private readonly string userId2 = Guid.NewGuid().ToString();

        public MongoDbSubscriptionRepositoryFixture _ { get; }

        public MongoDbSubscriptionRepositoryTests(MongoDbSubscriptionRepositoryFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_find_most_concrete_subscriptions()
        {
            var eventTopic = $"{topic}/child";

            var subscriptionTopic1 = topic;
            var subscriptionTopic2 = $"{topic}/child";

            await SubscribeAsync(userId1, subscriptionTopic1);
            await SubscribeAsync(userId1, subscriptionTopic2, true);

            await SubscribeAsync(userId2, subscriptionTopic1);
            await SubscribeAsync(userId2, subscriptionTopic2, true);

            var subscriptions = await ToList(_.Repository.QueryAsync(appId, eventTopic, null, default));

            Assert.Equal(2, subscriptions.Count);
            Assert.Equal(subscriptionTopic2, subscriptions[0].TopicPrefix);
            Assert.Equal(subscriptionTopic2, subscriptions[1].TopicPrefix);
        }

        [Fact]
        public async Task Should_find_same_subscription()
        {
            string eventTopic = topic, subscriptionTopic = topic;

            await SubscribeAsync(userId1, subscriptionTopic);

            var subscriptions = await ToList(_.Repository.QueryAsync(appId, eventTopic, null, default));

            Assert.Single(subscriptions);
            Assert.Equal(subscriptionTopic, subscriptions[0].TopicPrefix);
        }

        [Fact]
        public async Task Should_find_parent_subscription()
        {
            string eventTopic = $"{topic}/child", subscriptionTopic = topic;

            await SubscribeAsync(userId1, subscriptionTopic);

            var subscriptions = await ToList(_.Repository.QueryAsync(appId, eventTopic, null, default));

            Assert.Single(subscriptions);
            Assert.Equal(subscriptionTopic, subscriptions[0].TopicPrefix);
        }

        [Fact]
        public async Task Should_not_find_child_subscription()
        {
            string eventTopic = topic, subscriptionTopic = $"{topic}/child";

            await SubscribeAsync(userId1, subscriptionTopic);

            var subscriptions = await ToList(_.Repository.QueryAsync(appId, eventTopic, null, default));

            Assert.Empty(subscriptions);
        }

        [Fact]
        public async Task Should_unsubscribe_by_topic()
        {
            await SubscribeAsync(userId1, "tenant1/updates");
            await SubscribeAsync(userId1, "tenant1/updates/news");

            var subscriptions_0 = await QuerySubscriptionTopics(userId1);

            Assert.Equal(new string[]
            {
                "tenant1/updates",
                "tenant1/updates/news"
            }, subscriptions_0);

            await _.Repository.DeleteAsync(appId, userId1, "tenant1/updates", default);

            var subscriptions_1 = await QuerySubscriptionTopics(userId1);

            Assert.Equal(new string[]
            {
                "tenant1/updates/news"
            }, subscriptions_1);
        }

        [Fact]
        public async Task Should_unsubscribe_by_prefix()
        {
            await SubscribeAsync(userId1, "tenant1/updates");
            await SubscribeAsync(userId1, "tenant1/updates/news");
            await SubscribeAsync(userId1, "tenant2/updates");
            await SubscribeAsync(userId1, "tenant2/updates/news");

            var subscriptions_0 = await QuerySubscriptionTopics(userId1);

            Assert.Equal(new string[]
            {
                "tenant1/updates",
                "tenant1/updates/news",
                "tenant2/updates",
                "tenant2/updates/news"
            }, subscriptions_0);

            await _.Repository.DeletePrefixAsync(appId, userId1, "tenant2", default);

            var subscriptions_1 = await QuerySubscriptionTopics(userId1);

            Assert.Equal(new string[]
            {
                "tenant1/updates",
                "tenant1/updates/news"
            }, subscriptions_1);
        }

        private async Task<string[]> QuerySubscriptionTopics(string userId)
        {
            var subscriptions = await _.Repository.QueryAsync(appId, new SubscriptionQuery { UserId = userId }, default);

            return subscriptions.Select(x => x.TopicPrefix.ToString()).OrderBy(x => x).ToArray();
        }

        private Task SubscribeAsync(string userId, string topicPrefix, bool sendEmail = false)
        {
            var subscription = new Subscription
            {
                UserId = userId,
                TopicPrefix = topicPrefix,
                TopicSettings = new NotificationSettings()
            };

            if (sendEmail)
            {
                subscription.TopicSettings = new NotificationSettings
                {
                    [Providers.Email] = new NotificationSetting
                    {
                        Send = NotificationSend.Send
                    }
                };
            }

            return _.Repository.UpsertAsync(subscription, null, default);
        }

        private static async Task<List<T>> ToList<T>(IAsyncEnumerable<T> enumerable)
        {
            var list = new List<T>();

            await foreach (var item in enumerable)
            {
                list.Add(item);
            }

            return list;
        }
    }
}
