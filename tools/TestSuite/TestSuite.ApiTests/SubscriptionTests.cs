// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class SubscriptionTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; set; }

    public SubscriptionTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Theory]
    [InlineData(ClientMode.ClientId)]
    [InlineData(ClientMode.ApiKey)]
    public async Task Should_subscribe_user_to_topic(ClientMode mode)
    {
        var client = _.GetClient(mode);

        // STEP 0: Create user.
        var user_0 = await _.CreateUserAsync();

        var topicPath = $"news/{Guid.NewGuid()}";


        // STEP 1: Subscribe to topic.
        var subscribeRequest = new SubscribeManyDto
        {
            Subscribe =
            [
                new SubscribeDto
                {
                    TopicPrefix = topicPath,
                    TopicSettings = new Dictionary<string, ChannelSettingDto>
                    {
                        [Providers.WebPush] = new ChannelSettingDto
                        {
                            Send = ChannelSend.Send
                        }
                    }
                },
            ]
        };

        await client.Users.PostSubscriptionsAsync(_.AppId, user_0.Id, subscribeRequest);


        // STEP 2: Query subscriptions.
        var subscriptions = await client.Users.GetSubscriptionsAsync(_.AppId, user_0.Id, topicPath);

        var subscription = subscriptions.Items.SingleOrDefault(x => x.TopicPrefix == topicPath);

        Assert.NotNull(subscription);
        Assert.Equal(ChannelSend.Send, subscription.TopicSettings[Providers.WebPush].Send);
    }

    [Fact]
    public async Task Should_unsubscribe_user_from_topic()
    {
        // STEP 0: Create user.
        var user_0 = await _.CreateUserAsync();

        var topicPath1 = $"news/{Guid.NewGuid()}";
        var topicPath2 = $"news/{Guid.NewGuid()}";
        var topicPath3 = $"news/{Guid.NewGuid()}";


        // STEP 1: Subscribe to topics.
        var subscribeRequest = new SubscribeManyDto
        {
            Subscribe =
            [
                new SubscribeDto { TopicPrefix = topicPath1 },
                new SubscribeDto { TopicPrefix = topicPath2 },
                new SubscribeDto { TopicPrefix = topicPath3 },
            ]
        };

        await _.Client.Users.PostSubscriptionsAsync(_.AppId, user_0.Id, subscribeRequest);


        // STEP 2: Unsubscribe from the first topic with the batch endpoint.
        var unsubscribeRequest = new SubscribeManyDto
        {
            Unsubscribe =
            [
                topicPath1,
            ]
        };

        await _.Client.Users.PostSubscriptionsAsync(_.AppId, user_0.Id, unsubscribeRequest);


        // STEP 3: Unsubscribe from the second topic with the delete endpoint.
        await _.Client.Users.DeleteSubscriptionAsync(_.AppId, user_0.Id, topicPath2);


        // Get subscriptions.
        var subscriptions = await _.Client.Users.GetSubscriptionsAsync(_.AppId, user_0.Id, take: 100000);

        Assert.DoesNotContain(subscriptions.Items, x => x.TopicPrefix == topicPath1);
        Assert.DoesNotContain(subscriptions.Items, x => x.TopicPrefix == topicPath2);
        Assert.Contains(subscriptions.Items, x => x.TopicPrefix == topicPath3);
    }

    [Fact]
    public async Task Should_manage_own_subscriptions()
    {
        // STEP 0: Create user.
        var user_0 = await _.CreateUserAsync();

        var client = _.BuildUserClient(user_0);

        var topicPath = $"news/{Guid.NewGuid()}";


        // STEP 1: Subscribe to topic.
        var subscribeRequest = new SubscribeManyDto
        {
            Subscribe =
            [
                new SubscribeDto
                {
                    TopicPrefix = topicPath,
                    TopicSettings = new Dictionary<string, ChannelSettingDto>
                    {
                        [Providers.WebPush] = new ChannelSettingDto
                        {
                            Send = ChannelSend.Send
                        }
                    }
                },
            ]
        };

        await client.User.PostMySubscriptionsAsync(subscribeRequest);


        // STEP 2: Query own subscriptions.
        var subscriptions_0 = await client.User.GetMySubscriptionsAsync(topicPath);

        Assert.Contains(subscriptions_0.Items, x => x.TopicPrefix == topicPath);


        // STEP 3: Query single subscription.
        var subscription = await client.User.GetMySubscriptionAsync(topicPath);

        Assert.Equal(topicPath, subscription.TopicPrefix);
        Assert.Equal(ChannelSend.Send, subscription.TopicSettings[Providers.WebPush].Send);


        // STEP 4: Unsubscribe.
        await client.User.DeleteSubscriptionAsync(topicPath);

        var subscriptions_1 = await client.User.GetMySubscriptionsAsync(topicPath);

        Assert.DoesNotContain(subscriptions_1.Items, x => x.TopicPrefix == topicPath);
    }

    [Fact]
    public async Task Should_send_notification_to_topic_subscribers()
    {
        var subject = Guid.NewGuid().ToString();

        var topicPath = $"news/{Guid.NewGuid()}";

        // STEP 0: Create users and subscribe two of them.
        var user_0 = await _.CreateUserAsync();
        var user_1 = await _.CreateUserAsync();
        var user_2 = await _.CreateUserAsync();

        var subscribeRequest = new SubscribeManyDto
        {
            Subscribe =
            [
                new SubscribeDto { TopicPrefix = topicPath },
            ]
        };

        await _.Client.Users.PostSubscriptionsAsync(_.AppId, user_0.Id, subscribeRequest);
        await _.Client.Users.PostSubscriptionsAsync(_.AppId, user_1.Id, subscribeRequest);


        // STEP 1: Publish to the topic.
        var publishRequest = new PublishManyDto
        {
            Requests =
            [
                new PublishDto
                {
                    Topic = topicPath,
                    Preformatted = new NotificationFormattingDto
                    {
                        Subject = new LocalizedText
                        {
                            ["en"] = subject
                        }
                    }
                },
            ]
        };

        await _.Client.Events.PostEventsAsync(_.AppId, publishRequest);


        // Test that both subscribers got the notification.
        var notifications_0 = await _.Client.Notifications.PollAsync(_.AppId, user_0.Id);
        var notifications_1 = await _.Client.Notifications.PollAsync(_.AppId, user_1.Id);

        Assert.Contains(notifications_0, x => x.Subject == subject);
        Assert.Contains(notifications_1, x => x.Subject == subject);


        // Test that the user without a subscription got nothing.
        var notifications_2 = await _.Client.Notifications.GetNotificationsAsync(_.AppId, user_2.Id);

        Assert.Empty(notifications_2.Items);
    }
}
