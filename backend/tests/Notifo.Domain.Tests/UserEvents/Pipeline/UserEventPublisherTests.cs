// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Counters;
using Notifo.Domain.Events;
using Notifo.Domain.Log;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.Templates;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Texts;
using Squidex.Messaging;
using Xunit;

namespace Notifo.Domain.UserEvents.Pipeline;

public class UserEventPublisherTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly ICounterService counters = A.Fake<ICounterService>();
    private readonly IEventStore eventStore = A.Fake<IEventStore>();
    private readonly ILogStore logStore = A.Fake<ILogStore>();
    private readonly IMessageBus messageBus = A.Fake<IMessageBus>();
    private readonly ISubscriptionStore subscriptionStore = A.Fake<ISubscriptionStore>();
    private readonly ITemplateStore templateStore = A.Fake<ITemplateStore>();
    private readonly IUserStore userStore = A.Fake<IUserStore>();
    private readonly Randomizer randomizer = A.Fake<Randomizer>();
    private readonly List<UserEventMessage> publishedUserEvents = new List<UserEventMessage>();
    private readonly UserEventPublisher sut;

    public UserEventPublisherTests()
    {
        ct = cts.Token;

        A.CallTo(() => messageBus.PublishAsync(A<UserEventMessage>._, A<string>._, A<CancellationToken>._))
            .Invokes(call => publishedUserEvents.Add(call.GetArgument<UserEventMessage>(0)!));

        var log = A.Fake<ILogger<UserEventPublisher>>();

        sut = new UserEventPublisher(counters, logStore, eventStore, subscriptionStore, templateStore, userStore, messageBus, log, randomizer);
    }

    [Fact]
    public async Task Should_not_produce_message_if_app_id_not_set()
    {
        var @event = CreateMinimumEvent();

        @event.AppId = null!;

        await sut.PublishAsync(@event, ct);

        A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, A<TopicId>._, @event.CreatorId, ct))
            .MustNotHaveHappened();

        A.CallTo(() => logStore.LogAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_produce_message_if_topic_not_set()
    {
        var @event = CreateMinimumEvent();

        @event.Topic = null!;

        await sut.PublishAsync(@event, ct);

        A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, A<TopicId>._, @event.CreatorId, ct))
            .MustNotHaveHappened();

        A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_produce_message_if_formatting_not_set()
    {
        var @event = CreateMinimumEvent();

        @event.Formatting = null!;

        await sut.PublishAsync(@event, ct);

        A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, A<TopicId>._, @event.CreatorId, ct))
            .MustNotHaveHappened();

        A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_produce_message_if_subject_not_set()
    {
        var @event = CreateMinimumEvent();

        @event.Formatting = null!;

        await sut.PublishAsync(@event, ct);

        A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, A<TopicId>._, @event.CreatorId, ct))
            .MustNotHaveHappened();

        A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_produce_message_if_no_subscription_found()
    {
        var @event = CreateMinimumEvent();

        await sut.PublishAsync(@event, ct);

        Assert.Empty(publishedUserEvents);

        A.CallTo(() => eventStore.InsertAsync(@event, ct))
            .MustNotHaveHappened();

        A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_produce_message_to_subscriptions()
    {
        var @event = CreateMinimumEvent();

        @event.Properties = new NotificationProperties();

        var subscriptions = new[]
        {
            new Subscription
            {
                AppId = @event.AppId,
                TopicPrefix = "updates/sport",
                TopicSettings = new ChannelSettings
                {
                    ["sms"] = new ChannelSetting()
                },
                UserId = "123"
            },
            new Subscription
            {
                AppId = @event.AppId,
                TopicPrefix = "updates",
                TopicSettings = new ChannelSettings
                {
                    ["web"] = new ChannelSetting()
                },
                UserId = "456"
            }
        };

        A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, new TopicId(@event.Topic), @event.CreatorId, ct))
            .Returns(CreateAsyncEnumerable(subscriptions));

        await sut.PublishAsync(@event, ct);

        publishedUserEvents.Should().BeEquivalentTo(new List<UserEventMessage>
        {
            FromEvent(new UserEventMessage
            {
                Settings = subscriptions[0].TopicSettings,
                Topic = @event.Topic,
                TopicPrefix = subscriptions[0].TopicPrefix,
                UserId = subscriptions[0].UserId
            }, @event),
            FromEvent(new UserEventMessage
            {
                Settings = subscriptions[1].TopicSettings,
                Topic = @event.Topic,
                TopicPrefix = subscriptions[1].TopicPrefix,
                UserId = subscriptions[1].UserId
            }, @event)
        });

        A.CallTo(() => eventStore.InsertAsync(@event, ct))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => logStore.LogAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_create_empy_properties_if_not_set_in_event()
    {
        var @event = CreateMinimumEvent();

        @event.Properties = null;

        var subscriptions = new[]
        {
            new Subscription
            {
                TopicPrefix = "updates/sport",
                TopicSettings = new ChannelSettings(),
                UserId = "123"
            }
        };

        A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, new TopicId(@event.Topic), @event.CreatorId, ct))
            .Returns(CreateAsyncEnumerable(subscriptions));

        await sut.PublishAsync(@event, ct);

        Assert.Null(publishedUserEvents[0].Properties);

        A.CallTo(() => logStore.LogAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_create_empty_subscription_settings_if_not_set_in_subscription()
    {
        var @event = CreateMinimumEvent();

        var subscriptions = new[]
        {
            new Subscription
            {
                AppId = @event.AppId,
                TopicPrefix = "updates/sport",
                TopicSettings = null!,
                UserId = "123"
            }
        };

        A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, new TopicId(@event.Topic), @event.CreatorId, ct))
            .Returns(CreateAsyncEnumerable(subscriptions));

        await sut.PublishAsync(@event, ct);

        Assert.NotNull(publishedUserEvents[0].Settings);

        A.CallTo(() => logStore.LogAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData("user/123")]
    [InlineData("users/123")]
    public async Task Should_produce_directly_to_user_topic(string topic)
    {
        var @event = CreateMinimumEvent();

        @event.Topic = topic;

        await sut.PublishAsync(@event, ct);

        publishedUserEvents.Should().BeEquivalentTo(new List<UserEventMessage>
        {
            FromEvent(new UserEventMessage
            {
                Topic = @event.Topic,
                TopicPrefix = "users/123",
                UserId = "123"
            }, @event)
        });

        A.CallTo(() => logStore.LogAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData("user/all")]
    [InlineData("users/all")]
    public async Task Should_produce_directly_to_all_users_topic(string topic)
    {
        var @event = CreateMinimumEvent();

        @event.Topic = topic;

        var userIds = new[]
        {
            "123",
            "456"
        };

        A.CallTo(() => userStore.QueryIdsAsync(@event.AppId, ct))
            .Returns(CreateAsyncEnumerable(userIds));

        await sut.PublishAsync(@event, ct);

        publishedUserEvents.Should().BeEquivalentTo(new List<UserEventMessage>
        {
            FromEvent(new UserEventMessage
            {
                Topic = @event.Topic,
                TopicPrefix = "users/all",
                UserId = "123"
            }, @event),
            FromEvent(new UserEventMessage
            {
                Topic = @event.Topic,
                TopicPrefix = "users/all",
                UserId = "456"
            }, @event)
        });

        A.CallTo(() => subscriptionStore.QueryAsync(A<string>._, A<TopicId>._, A<string>._, ct))
            .MustNotHaveHappened();

        A.CallTo(() => logStore.LogAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_produce_message_if_event_already_in_store()
    {
        var @event = CreateMinimumEvent();

        var subscriptions = new[]
        {
            new Subscription
            {
                AppId = @event.AppId,
                TopicPrefix = "updates/sport",
                TopicSettings = new ChannelSettings(),
                UserId = "123"
            }
        };

        A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, new TopicId(@event.Topic), @event.CreatorId, ct))
            .Returns(CreateAsyncEnumerable(subscriptions));

        A.CallTo(() => eventStore.InsertAsync(@event, ct))
            .Throws(new UniqueConstraintException());

        await sut.PublishAsync(@event, ct);

        Assert.Empty(publishedUserEvents);

        A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_produce_message_to_subscriptions_with_data_from_template()
    {
        var @event = CreateMinimumEvent();

        @event.Data = "123";
        @event.Formatting = null;
        @event.Silent = true;
        @event.TemplateCode = "TEMPL";

        var template = CreateMinimumTemplate();

        A.CallTo(() => templateStore.GetAsync(@event.AppId, @event.TemplateCode, ct))
            .Returns(template);

        var subscriptions = new[]
        {
            new Subscription
            {
                AppId = @event.AppId,
                TopicPrefix = "updates/sport",
                TopicSettings = new ChannelSettings(),
                UserId = "123"
            },
            new Subscription
            {
                AppId = @event.AppId,
                TopicPrefix = "updates",
                TopicSettings = null!,
                UserId = "456"
            }
        };

        A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, new TopicId(@event.Topic), @event.CreatorId, ct))
            .Returns(CreateAsyncEnumerable(subscriptions));

        await sut.PublishAsync(@event, ct);

        publishedUserEvents.Should().BeEquivalentTo(new List<UserEventMessage>
        {
            FromEvent(new UserEventMessage
            {
                Formatting = template.Formatting.Format(@event.Properties!),
                Topic = @event.Topic,
                TopicPrefix = subscriptions[0].TopicPrefix,
                UserId = subscriptions[0].UserId
            }, @event),
            FromEvent(new UserEventMessage
            {
                Formatting = template.Formatting.Format(@event.Properties),
                Topic = @event.Topic,
                TopicPrefix = subscriptions[1].TopicPrefix,
                UserId = subscriptions[1].UserId
            }, @event)
        });

        A.CallTo(() => eventStore.InsertAsync(@event, ct))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => templateStore.GetAsync(@event.AppId, @event.TemplateCode, ct))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => logStore.LogAsync(A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_produce_message_if_template_autogenerated()
    {
        var @event = CreateMinimumEvent();

        @event.TemplateCode = "TEMPL";
        @event.Formatting = null;

        var template = CreateMinimumTemplate(true);

        A.CallTo(() => templateStore.GetAsync(@event.AppId, @event.TemplateCode, ct))
            .Returns(template);

        var subscriptions = new[]
        {
            new Subscription
            {
                AppId = @event.AppId,
                TopicPrefix = "updates/sport",
                TopicSettings = new ChannelSettings(),
                UserId = "123"
            }
        };

        A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, new TopicId(@event.Topic), @event.CreatorId, ct))
            .Returns(CreateAsyncEnumerable(subscriptions));

        await sut.PublishAsync(@event, ct);

        Assert.Empty(publishedUserEvents);

        A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_retrieve_template_if_no_subscription_found()
    {
        var @event = CreateMinimumEvent();

        @event.TemplateCode = "TEMPL";
        @event.Formatting = null;

        await sut.PublishAsync(@event, ct);

        Assert.Empty(publishedUserEvents);

        A.CallTo(() => eventStore.InsertAsync(@event, ct))
            .MustNotHaveHappened();

        A.CallTo(() => templateStore.GetAsync(@event.AppId, @event.TemplateCode, ct))
            .MustNotHaveHappened();

        A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
            .MustHaveHappened();
    }

    [Theory]
    [InlineData(0.0, "TEMPL1")]
    [InlineData(0.2, "TEMPL1")]
    [InlineData(0.4, "TEMPL1")]
    [InlineData(0.5, "TEMPL2")]
    [InlineData(0.6, "TEMPL2")]
    [InlineData(1.0, "TEMPL3")]
    public async Task Should_get_template_on_propability(double probability, string expectedCode)
    {
        var @event = CreateMinimumEvent();

        @event.Topic = "users/123";
        @event.TemplateCode = "TEMPL3";
        @event.TemplateVariants = new Dictionary<string, double>
        {
            ["TEMPL1"] = 0.4,
            ["TEMPL2"] = 0.2
        };

        A.CallTo(() => randomizer.NextDouble())
            .Returns(probability);

        await sut.PublishAsync(@event, ct);

        A.CallTo(() => templateStore.GetAsync(@event.AppId, expectedCode, ct))
            .MustHaveHappened();
    }

    private static async IAsyncEnumerable<T> CreateAsyncEnumerable<T>(params T[] source)
    {
        foreach (var item in source)
        {
            await Task.Yield();
            yield return item;
        }
    }

    private static UserEventMessage FromEvent(UserEventMessage userEventMessage, EventMessage @event)
    {
        userEventMessage.AppId = @event.AppId;
        userEventMessage.Data = @event.Data;
        userEventMessage.EventActivity = @event.EventActivity;
        userEventMessage.EventId = @event.Id;
        userEventMessage.Formatting ??= @event.Formatting!;
        userEventMessage.Properties ??= @event.Properties;
        userEventMessage.Settings ??= new ChannelSettings();
        userEventMessage.Silent = @event.Silent;
        userEventMessage.Topic ??= @event.Topic;

        return userEventMessage;
    }

    private static Template CreateMinimumTemplate(bool isAutoCreated = false)
    {
        return new Template("1", "1", default)
        {
            AppId = "app",
            Formatting = new NotificationFormatting<LocalizedText>
            {
                Subject = new LocalizedText
                {
                    ["de"] = "Test"
                }
            },
            IsAutoCreated = isAutoCreated
        };
    }

    private static EventMessage CreateMinimumEvent()
    {
        return new EventMessage
        {
            AppId = "app",
            Formatting = new NotificationFormatting<LocalizedText>
            {
                Subject = new LocalizedText
                {
                    ["de"] = "Test"
                }
            },
            Properties = new NotificationProperties
            {
                ["property1"] = "value1"
            },
            Topic = "updates/stort"
        };
    }
}
