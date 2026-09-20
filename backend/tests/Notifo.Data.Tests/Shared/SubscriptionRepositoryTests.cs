// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain;
using Notifo.Domain.Integrations;
using Notifo.Domain.Subscriptions;
using Notifo.Infrastructure;

namespace Notifo.Shared;

public abstract class SubscriptionRepositoryTests
{
    private readonly string topic = Guid.NewGuid().ToString();
    private readonly string appId = Guid.NewGuid().ToString();
    private readonly string userId1 = Guid.NewGuid().ToString();
    private readonly string userId2 = Guid.NewGuid().ToString();

    protected abstract Task<ISubscriptionRepository> CreateSutAsync();

    [Fact]
    public async Task Should_find_most_concrete_subscriptions()
    {
        var sut = await CreateSutAsync();

        var eventTopic = $"{topic}/child";

        var subscriptionTopic1 = topic;
        var subscriptionTopic2 = $"{topic}/child";

        await SubscribeAsync(sut, userId1, subscriptionTopic1);
        await SubscribeAsync(sut, userId1, subscriptionTopic2, true);

        await SubscribeAsync(sut, userId2, subscriptionTopic1);
        await SubscribeAsync(sut, userId2, subscriptionTopic2, true);

        var subscriptions = await ToList(sut.QueryAsync(appId, eventTopic));

        Assert.Equal(2, subscriptions.Count);
        Assert.Equal(subscriptionTopic2, subscriptions[0].TopicPrefix);
        Assert.Equal(subscriptionTopic2, subscriptions[1].TopicPrefix);
    }

    [Fact]
    public async Task Should_find_same_subscription()
    {
        var sut = await CreateSutAsync();

        string eventTopic = topic, subscriptionTopic = topic;

        await SubscribeAsync(sut, userId1, subscriptionTopic);

        var subscriptions = await ToList(sut.QueryAsync(appId, eventTopic));

        Assert.Single(subscriptions);
        Assert.Equal(subscriptionTopic, subscriptions[0].TopicPrefix);
    }

    [Fact]
    public async Task Should_find_parent_subscription()
    {
        var sut = await CreateSutAsync();

        string eventTopic = $"{topic}/child", subscriptionTopic = topic;

        await SubscribeAsync(sut, userId1, subscriptionTopic);

        var subscriptions = await ToList(sut.QueryAsync(appId, eventTopic));

        Assert.Single(subscriptions);
        Assert.Equal(subscriptionTopic, subscriptions[0].TopicPrefix);
    }

    [Fact]
    public async Task Should_not_find_child_subscription()
    {
        var sut = await CreateSutAsync();

        string eventTopic = topic, subscriptionTopic = $"{topic}/child";

        await SubscribeAsync(sut, userId1, subscriptionTopic);

        var subscriptions = await ToList(sut.QueryAsync(appId, eventTopic));

        Assert.Empty(subscriptions);
    }

    [Fact]
    public async Task Should_not_find_subscription_of_excluded_user()
    {
        var sut = await CreateSutAsync();

        await SubscribeAsync(sut, userId1, topic);
        await SubscribeAsync(sut, userId2, topic);

        var subscriptions = await ToList(sut.QueryAsync(appId, topic, userId1));

        Assert.Equal([userId2], subscriptions.Select(x => x.UserId));
    }

    [Fact]
    public async Task Should_not_find_subscription_of_other_app()
    {
        var sut = await CreateSutAsync();

        await SubscribeAsync(sut, userId1, topic);

        var subscriptions = await ToList(sut.QueryAsync(Guid.NewGuid().ToString(), topic));

        Assert.Empty(subscriptions);
    }

    [Fact]
    public async Task Should_unsubscribe_by_topic()
    {
        var sut = await CreateSutAsync();

        await SubscribeAsync(sut, userId1, "tenant1/updates");
        await SubscribeAsync(sut, userId1, "tenant1/updates/news");

        var subscriptions_0 = await QuerySubscriptionTopics(sut, userId1);

        Assert.Equal(new[]
        {
            "tenant1/updates",
            "tenant1/updates/news"
        }, subscriptions_0);

        await sut.DeleteAsync(appId, userId1, "tenant1/updates", default);

        var subscriptions_1 = await QuerySubscriptionTopics(sut, userId1);

        Assert.Equal(new[]
        {
            "tenant1/updates/news"
        }, subscriptions_1);
    }

    [Fact]
    public async Task Should_unsubscribe_by_prefix()
    {
        var sut = await CreateSutAsync();

        await SubscribeAsync(sut, userId1, "tenant1/updates");
        await SubscribeAsync(sut, userId1, "tenant1/updates/news");
        await SubscribeAsync(sut, userId1, "tenant2/updates");
        await SubscribeAsync(sut, userId1, "tenant2/updates/news");

        var subscriptions_0 = await QuerySubscriptionTopics(sut, userId1);

        Assert.Equal(new[]
        {
            "tenant1/updates",
            "tenant1/updates/news",
            "tenant2/updates",
            "tenant2/updates/news"
        }, subscriptions_0);

        await sut.DeletePrefixAsync(appId, userId1, "tenant2", default);

        var subscriptions_1 = await QuerySubscriptionTopics(sut, userId1);

        Assert.Equal(new[]
        {
            "tenant1/updates",
            "tenant1/updates/news"
        }, subscriptions_1);
    }

    [Fact]
    public async Task Should_not_unsubscribe_other_user_by_prefix()
    {
        var sut = await CreateSutAsync();

        await SubscribeAsync(sut, userId1, "tenant1/updates");
        await SubscribeAsync(sut, userId2, "tenant1/updates");

        await sut.DeletePrefixAsync(appId, userId1, "tenant1", default);

        Assert.Empty(await QuerySubscriptionTopics(sut, userId1));
        Assert.Equal(["tenant1/updates"], await QuerySubscriptionTopics(sut, userId2));
    }

    [Fact]
    public async Task Should_store_scheduling()
    {
        var sut = await CreateSutAsync();

        var scheduling = new Scheduling
        {
            Type = SchedulingType.UTC,
            Time = new LocalTime(8, 0),
            NextWeekDay = IsoDayOfWeek.Monday
        };

        await sut.UpsertAsync(new Subscription
        {
            AppId = appId,
            UserId = userId1,
            TopicPrefix = "news",
            TopicSettings = [],
            Scheduling = scheduling
        });

        var subscriptions = await sut.QueryAsync(appId, new SubscriptionQuery { UserId = userId1 }, default);

        subscriptions.Single().Scheduling.Should().BeEquivalentTo(scheduling);
    }

    [Fact]
    public async Task Should_not_insert_deleted_subscription_again()
    {
        var sut = await CreateSutAsync();

        await SubscribeAsync(sut, userId1, topic);

        var (_, etag) = await sut.GetAsync(appId, userId1, topic, default);

        await sut.DeleteAsync(appId, userId1, topic, default);

        var subscription = new Subscription
        {
            AppId = appId,
            UserId = userId1,
            TopicPrefix = topic,
            TopicSettings = []
        };

        await Assert.ThrowsAsync<InconsistentStateException>(() => sut.UpsertAsync(subscription, etag));

        var subscriptions = await sut.QueryAsync(appId, new SubscriptionQuery { UserId = userId1 }, default);

        Assert.Empty(subscriptions);
    }

    [Fact]
    public async Task Should_throw_exception_if_query_is_cancelled()
    {
        var sut = await CreateSutAsync();

        await SubscribeAsync(sut, userId1, topic);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ToList(sut.QueryAsync(appId, topic, null, cts.Token)));
    }

    [Fact]
    public async Task Should_insert_and_get_subscription()
    {
        var sut = await CreateSutAsync();

        var subscription = CreateSubscription(userId1, topic);

        await sut.UpsertAsync(subscription);

        var (result, etag) = await sut.GetAsync(appId, userId1, topic);

        result.Should().BeEquivalentTo(subscription);
        Assert.NotNull(etag);
    }

    [Fact]
    public async Task Should_return_null_if_subscription_not_found()
    {
        var sut = await CreateSutAsync();

        var (result, etag) = await sut.GetAsync(appId, userId1, topic);

        Assert.Null(result);
        Assert.Null(etag);
    }

    [Fact]
    public async Task Should_update_subscription_with_etag()
    {
        var sut = await CreateSutAsync();

        var subscription = CreateSubscription(userId1, topic);

        await sut.UpsertAsync(subscription);

        var (_, etag) = await sut.GetAsync(appId, userId1, topic);

        var updated = subscription with
        {
            TopicSettings = new ChannelSettings
            {
                [Providers.Sms] = new ChannelSetting
                {
                    Send = ChannelSend.NotSending
                }
            }
        };

        await sut.UpsertAsync(updated, etag);

        var (result, newEtag) = await sut.GetAsync(appId, userId1, topic);

        result.Should().BeEquivalentTo(updated);
        Assert.NotEqual(etag, newEtag);
    }

    [Fact]
    public async Task Should_throw_exception_if_etag_does_not_match()
    {
        var sut = await CreateSutAsync();

        var subscription = CreateSubscription(userId1, topic);

        await sut.UpsertAsync(subscription);

        await Assert.ThrowsAsync<InconsistentStateException>(() => sut.UpsertAsync(subscription, "invalid"));
    }

    [Fact]
    public async Task Should_query_subscriptions_by_topics()
    {
        var sut = await CreateSutAsync();

        await SubscribeAsync(sut, userId1, "news");
        await SubscribeAsync(sut, userId1, "news/sport");
        await SubscribeAsync(sut, userId1, "weather");

        var result = await sut.QueryAsync(appId, new SubscriptionQuery { Topics = ["news", "weather"] });

        Assert.Equal(["news", "weather"], result.Select(x => x.TopicPrefix.Id).Order());
    }

    [Fact]
    public async Task Should_query_subscriptions_by_text()
    {
        var sut = await CreateSutAsync();

        await SubscribeAsync(sut, userId1, "news/sport");
        await SubscribeAsync(sut, userId1, "news/politics");

        var result = await sut.QueryAsync(appId, new SubscriptionQuery { Query = "SPORT" });

        Assert.Equal(["news/sport"], result.Select(x => x.TopicPrefix.Id));
    }

    [Fact]
    public async Task Should_query_subscriptions_by_user()
    {
        var sut = await CreateSutAsync();

        await SubscribeAsync(sut, userId1, "news");
        await SubscribeAsync(sut, userId2, "news");

        var result = await sut.QueryAsync(appId, new SubscriptionQuery { UserId = userId2 });

        Assert.Equal([userId2], result.Select(x => x.UserId));
    }

    [Fact]
    public async Task Should_query_subscriptions_with_paging_and_total()
    {
        var sut = await CreateSutAsync();

        await SubscribeAsync(sut, userId1, "topic1");
        await SubscribeAsync(sut, userId1, "topic2");
        await SubscribeAsync(sut, userId1, "topic3");

        var result = await sut.QueryAsync(appId, new SubscriptionQuery { Take = 2 });

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.Total);
    }

    [Fact]
    public async Task Should_not_query_subscriptions_of_other_app()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateSubscription(userId1, topic) with { AppId = Guid.NewGuid().ToString() });

        var result = await sut.QueryAsync(appId, new SubscriptionQuery());

        Assert.Empty(result);
    }

    protected static async Task<List<T>> ToList<T>(IAsyncEnumerable<T> enumerable)
    {
        var list = new List<T>();

        await foreach (var item in enumerable)
        {
            list.Add(item);
        }

        return list;
    }

    private async Task<string[]> QuerySubscriptionTopics(ISubscriptionRepository sut, string userId)
    {
        var subscriptions = await sut.QueryAsync(appId, new SubscriptionQuery { UserId = userId }, default);

        return subscriptions.Select(x => x.TopicPrefix.ToString()).Order().ToArray();
    }

    private Task SubscribeAsync(ISubscriptionRepository sut, string userId, string topicPrefix, bool sendEmail = false)
    {
        var subscription = new Subscription
        {
            AppId = appId,
            UserId = userId,
            TopicPrefix = topicPrefix,
            TopicSettings = []
        };

        if (sendEmail)
        {
            subscription.TopicSettings[Providers.Email] = new ChannelSetting
            {
                Send = ChannelSend.Send
            };
        }

        return sut.UpsertAsync(subscription);
    }

    private Subscription CreateSubscription(string userId, string topicPrefix)
    {
        return new Subscription
        {
            AppId = appId,
            UserId = userId,
            TopicPrefix = topicPrefix,
            TopicSettings = new ChannelSettings
            {
                [Providers.Email] = new ChannelSetting
                {
                    Send = ChannelSend.Send
                }
            }
        };
    }
}
