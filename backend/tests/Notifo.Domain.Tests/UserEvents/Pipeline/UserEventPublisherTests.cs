// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Domain.Events;
using Notifo.Domain.Log;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.Templates;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Messaging;
using Notifo.Infrastructure.Texts;
using Squidex.Log;
using Xunit;

namespace Notifo.Domain.UserEvents.Pipeline
{
    public class UserEventPublisherTests
    {
        private readonly ISubscriptionStore subscriptionStore = A.Fake<ISubscriptionStore>();
        private readonly ICounterService counters = A.Fake<ICounterService>();
        private readonly ITemplateStore templateStore = A.Fake<ITemplateStore>();
        private readonly ILogStore logStore = A.Fake<ILogStore>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IUserStore userStore = A.Fake<IUserStore>();
        private readonly IAbstractProducer<UserEventMessage> producer = A.Fake<IAbstractProducer<UserEventMessage>>();
        private readonly List<UserEventMessage> publishedUserEvents = new List<UserEventMessage>();
        private readonly UserEventPublisher sut;

        public UserEventPublisherTests()
        {
            A.CallTo(() => producer.ProduceAsync(A<string>._, A<UserEventMessage>._))
                .Invokes(call => publishedUserEvents.Add(call.GetArgument<UserEventMessage>(1)!));

            sut = new UserEventPublisher(counters, logStore, eventStore, subscriptionStore, templateStore, userStore, producer, A.Fake<ISemanticLog>(), A.Fake<IClock>());
        }

        [Fact]
        public async Task Should_not_produce_message_if_app_id_not_set()
        {
            var @event = CreateMinimumEvent();

            @event.AppId = null!;

            await sut.PublishAsync(@event, default);

            A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, A<TopicId>._, @event.CreatorId, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => logStore.LogAsync(A<string>._, A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_produce_message_if_topic_not_set()
        {
            var @event = CreateMinimumEvent();

            @event.Topic = null!;

            await sut.PublishAsync(@event, default);

            A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, A<TopicId>._, @event.CreatorId, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_produce_message_if_formatting_not_set()
        {
            var @event = CreateMinimumEvent();

            @event.Formatting = null!;

            await sut.PublishAsync(@event, default);

            A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, A<TopicId>._, @event.CreatorId, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_produce_message_if_subject_not_set()
        {
            var @event = CreateMinimumEvent();

            @event.Formatting = null!;

            await sut.PublishAsync(@event, default);

            A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, A<TopicId>._, @event.CreatorId, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_produce_message_if_no_subscription_found()
        {
            var @event = CreateMinimumEvent();

            await sut.PublishAsync(@event, default);

            Assert.Empty(publishedUserEvents);

            A.CallTo(() => eventStore.InsertAsync(@event, default))
                .MustNotHaveHappened();

            A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_produce_message_to_subscriptions()
        {
            var @event = CreateMinimumEvent();

            @event.Properties = new EventProperties();

            var subscriptions = new[]
            {
                new Subscription
                {
                    TopicPrefix = "updates/sport",
                    TopicSettings = new NotificationSettings(),
                    UserId = "123"
                },
                new Subscription
                {
                    TopicPrefix = "updates",
                    TopicSettings = null!,
                    UserId = "456"
                }
            };

            A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, new TopicId(@event.Topic), @event.CreatorId, A<CancellationToken>._))
                .Returns(CreateAsyncEnumerable(subscriptions));

            await sut.PublishAsync(@event, default);

            publishedUserEvents.Should().BeEquivalentTo(new List<UserEventMessage>
            {
                new UserEventMessage
                {
                    AppId = @event.AppId,
                    EventId = @event.Id,
                    EventSettings = @event.Settings,
                    Formatting = @event.Formatting!,
                    Properties = @event.Properties,
                    SubscriptionPrefix = subscriptions[0].TopicPrefix.ToString(),
                    SubscriptionSettings = subscriptions[0].TopicSettings,
                    Topic = @event.Topic,
                    UserId = subscriptions[0].UserId
                },
                new UserEventMessage
                {
                    AppId = @event.AppId,
                    EventId = @event.Id,
                    EventSettings = @event.Settings,
                    Formatting = @event.Formatting!,
                    Properties = @event.Properties,
                    SubscriptionPrefix = subscriptions[1].TopicPrefix.ToString(),
                    SubscriptionSettings = new NotificationSettings(),
                    Topic = @event.Topic,
                    UserId = subscriptions[1].UserId
                }
            });

            A.CallTo(() => eventStore.InsertAsync(@event, default))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => logStore.LogAsync(A<string>._, A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_create_empy_properties_if_not_set_in_event()
        {
            var @event = CreateMinimumEvent();

            var subscriptions = new[]
            {
                new Subscription
                {
                    TopicPrefix = "updates/sport",
                    TopicSettings = new NotificationSettings(),
                    UserId = "123"
                }
            };

            A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, new TopicId(@event.Topic), @event.CreatorId, A<CancellationToken>._))
                .Returns(CreateAsyncEnumerable(subscriptions));

            await sut.PublishAsync(@event, default);

            Assert.NotNull(publishedUserEvents[0].Properties);

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
                    TopicPrefix = "updates/sport",
                    TopicSettings = null!,
                    UserId = "123"
                }
            };

            A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, new TopicId(@event.Topic), @event.CreatorId, A<CancellationToken>._))
                .Returns(CreateAsyncEnumerable(subscriptions));

            await sut.PublishAsync(@event, default);

            Assert.NotNull(publishedUserEvents[0].SubscriptionSettings);

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

            await sut.PublishAsync(@event, default);

            publishedUserEvents.Should().BeEquivalentTo(new List<UserEventMessage>
            {
                new UserEventMessage
                {
                    AppId = @event.AppId,
                    EventId = @event.Id,
                    EventSettings = @event.Settings,
                    Formatting = @event.Formatting!,
                    Properties = new EventProperties(),
                    SubscriptionPrefix = "users/123",
                    SubscriptionSettings = new NotificationSettings(),
                    Topic = @event.Topic,
                    UserId = "123"
                }
            });

            A.CallTo(() => subscriptionStore.QueryAsync(A<string>._, A<TopicId>._, A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();

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

            A.CallTo(() => userStore.QueryIdsAsync(@event.AppId, default))
                .Returns(CreateAsyncEnumerable(userIds));

            await sut.PublishAsync(@event, default);

            publishedUserEvents.Should().BeEquivalentTo(new List<UserEventMessage>
            {
                new UserEventMessage
                {
                    AppId = @event.AppId,
                    EventId = @event.Id,
                    EventSettings = @event.Settings,
                    Formatting = @event.Formatting!,
                    Properties = new EventProperties(),
                    SubscriptionPrefix = "users/all",
                    SubscriptionSettings = new NotificationSettings(),
                    Topic = @event.Topic,
                    UserId = "123"
                },
                new UserEventMessage
                {
                    AppId = @event.AppId,
                    EventId = @event.Id,
                    EventSettings = @event.Settings,
                    Formatting = @event.Formatting!,
                    Properties = new EventProperties(),
                    SubscriptionPrefix = "users/all",
                    SubscriptionSettings = new NotificationSettings(),
                    Topic = @event.Topic,
                    UserId = "456"
                }
            });

            A.CallTo(() => subscriptionStore.QueryAsync(A<string>._, A<TopicId>._, A<string>._, A<CancellationToken>._))
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
                    TopicPrefix = "updates/sport",
                    TopicSettings = new NotificationSettings(),
                    UserId = "123"
                }
            };

            A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, new TopicId(@event.Topic), @event.CreatorId, A<CancellationToken>._))
                .Returns(CreateAsyncEnumerable(subscriptions));

            A.CallTo(() => eventStore.InsertAsync(@event, default))
                .Throws(new UniqueConstraintException());

            await sut.PublishAsync(@event, default);

            Assert.Empty(publishedUserEvents);

            A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_produce_message_to_subscriptions_with_data_from_template()
        {
            var @event = CreateMinimumEvent();

            @event.TemplateCode = "TEMPL";
            @event.Data = "123";
            @event.Formatting = null;
            @event.Silent = true;

            var template = CreateMinimumTemplate();

            A.CallTo(() => templateStore.GetAsync(@event.AppId, @event.TemplateCode, default))
                .Returns(template);

            var subscriptions = new[]
            {
                new Subscription
                {
                    TopicPrefix = "updates/sport",
                    TopicSettings = new NotificationSettings(),
                    UserId = "123"
                },
                new Subscription
                {
                    TopicPrefix = "updates",
                    TopicSettings = null!,
                    UserId = "456"
                }
            };

            A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, new TopicId(@event.Topic), @event.CreatorId, A<CancellationToken>._))
                .Returns(CreateAsyncEnumerable(subscriptions));

            await sut.PublishAsync(@event, default);

            publishedUserEvents.Should().BeEquivalentTo(new List<UserEventMessage>
            {
                new UserEventMessage
                {
                    AppId = @event.AppId,
                    Data = "123",
                    EventId = @event.Id,
                    EventSettings = @event.Settings,
                    Formatting = template.Formatting.Format(@event.Properties),
                    Properties = new EventProperties(),
                    Silent = true,
                    SubscriptionPrefix = subscriptions[0].TopicPrefix.ToString(),
                    SubscriptionSettings = subscriptions[0].TopicSettings,
                    Topic = @event.Topic,
                    UserId = subscriptions[0].UserId
                },
                new UserEventMessage
                {
                    AppId = @event.AppId,
                    Data = "123",
                    EventId = @event.Id,
                    EventSettings = @event.Settings,
                    Formatting = template.Formatting.Format(@event.Properties),
                    Properties = new EventProperties(),
                    Silent = true,
                    SubscriptionPrefix = subscriptions[1].TopicPrefix.ToString(),
                    SubscriptionSettings = new NotificationSettings(),
                    Topic = @event.Topic,
                    UserId = subscriptions[1].UserId
                }
            });

            A.CallTo(() => eventStore.InsertAsync(@event, default))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => templateStore.GetAsync(@event.AppId, @event.TemplateCode, default))
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

            var template = CreateMinimumTemplate();

            template.IsAutoCreated = true;

            A.CallTo(() => templateStore.GetAsync(@event.AppId, @event.TemplateCode, default))
                .Returns(template);

            var subscriptions = new[]
            {
                new Subscription
                {
                    TopicPrefix = "updates/sport",
                    TopicSettings = new NotificationSettings(),
                    UserId = "123"
                }
            };

            A.CallTo(() => subscriptionStore.QueryAsync(@event.AppId, new TopicId(@event.Topic), @event.CreatorId, A<CancellationToken>._))
                .Returns(CreateAsyncEnumerable(subscriptions));

            await sut.PublishAsync(@event, default);

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

            await sut.PublishAsync(@event, default);

            Assert.Empty(publishedUserEvents);

            A.CallTo(() => eventStore.InsertAsync(@event, default))
                .MustNotHaveHappened();

            A.CallTo(() => templateStore.GetAsync(@event.AppId, @event.TemplateCode, default))
                .MustNotHaveHappened();

            A.CallTo(() => logStore.LogAsync(@event.AppId, A<string>._))
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

        private static Template CreateMinimumTemplate()
        {
            var template = new Template
            {
                Formatting = new NotificationFormatting<LocalizedText>
                {
                    Subject = new LocalizedText
                    {
                        ["de"] = "Test"
                    }
                }
            };

            template.AppId = "app";

            return template;
        }

        private static EventMessage CreateMinimumEvent()
        {
            var @event = new EventMessage
            {
                Formatting = new NotificationFormatting<LocalizedText>
                {
                    Subject = new LocalizedText
                    {
                        ["de"] = "Test"
                    }
                }
            };

            @event.AppId = "app";
            @event.Topic = "updates/sport";

            return @event;
        }
    }
}
