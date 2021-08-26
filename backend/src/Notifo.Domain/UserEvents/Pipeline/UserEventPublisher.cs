// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Counters;
using Notifo.Domain.Events;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.Templates;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Reflection;
using IUserEventProducer = Notifo.Infrastructure.Messaging.IAbstractProducer<Notifo.Domain.UserEvents.UserEventMessage>;

namespace Notifo.Domain.UserEvents.Pipeline
{
    public sealed class UserEventPublisher : IUserEventPublisher
    {
        private static readonly HashSet<string> UserAllTopics = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "users/all",
            "user/all"
        };

        private static readonly string[] UserTopicPrefixex =
        {
            "users/",
            "user/"
        };

        private readonly IUserEventProducer userEventProducer;
        private readonly ICounterService counters;
        private readonly IEventStore eventStore;
        private readonly ILogStore logStore;
        private readonly ISubscriptionStore subscriptionStore;
        private readonly ITemplateStore templateStore;
        private readonly IUserStore userStore;

        public UserEventPublisher(ICounterService counters, ILogStore logStore,
            IEventStore eventStore,
            ISubscriptionStore subscriptionStore,
            ITemplateStore templateStore,
            IUserStore userStore,
            IUserEventProducer userEventProducer)
        {
            this.subscriptionStore = subscriptionStore;
            this.counters = counters;
            this.eventStore = eventStore;
            this.logStore = logStore;
            this.templateStore = templateStore;
            this.userStore = userStore;
            this.userEventProducer = userEventProducer;
        }

        public async Task PublishAsync(EventMessage message,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("PublishUserEvent"))
            {
                if (string.IsNullOrWhiteSpace(message.AppId))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(message.Topic))
                {
                    await logStore.LogAsync(message.AppId, Texts.Events_NoTopic);
                    return;
                }

                if (string.IsNullOrWhiteSpace(message.TemplateCode) && message.Formatting?.HasSubject() != true)
                {
                    await logStore.LogAsync(message.AppId, Texts.Events_NoSubjectOrTemplateCode);
                    return;
                }

                var count = 0;

                await foreach (var subscription in GetSubscriptions(message).WithCancellation(ct))
                {
                    if (count == 0)
                    {
                        if (!string.IsNullOrWhiteSpace(message.TemplateCode))
                        {
                            var template = await templateStore.GetAsync(message.AppId, message.TemplateCode, ct);

                            if (template?.IsAutoCreated == false)
                            {
                                message.Formatting = template.Formatting;

                                if (template.Settings?.Count > 0)
                                {
                                    var settings = new NotificationSettings();

                                    settings.OverrideBy(template.Settings);
                                    settings.OverrideBy(message.Settings);

                                    message.Settings = settings;
                                }
                            }
                        }

                        if (message.Formatting?.HasSubject() != true)
                        {
                            await logStore.LogAsync(message.AppId, string.Format(Texts.Template_NoSubject, message.TemplateCode));
                            return;
                        }

                        message.Formatting = message.Formatting.Format(message.Properties);

                        try
                        {
                            await eventStore.InsertAsync(message, ct);
                        }
                        catch (UniqueConstraintException)
                        {
                            await logStore.LogAsync(message.AppId, Texts.Events_AlreadyProcessed);
                            break;
                        }
                    }

                    var userEventMessage = CreateUserEventMessage(message, subscription);

                    await userEventProducer.ProduceAsync(subscription.UserId, userEventMessage);

                    count++;
                }

                if (count > 0)
                {
                    var counterMap = CounterMap.ForNotification(ProcessStatus.Attempt, count);
                    var counterKey = CounterKey.ForEvent(message);

                    await counters.CollectAsync(counterKey, counterMap, ct);
                }
                else
                {
                    await logStore.LogAsync(message.AppId, Texts.Events_NoSubscriber);
                }
            }
        }

        private async IAsyncEnumerable<Subscription> GetSubscriptions(EventMessage @event)
        {
            var topic = @event.Topic;

            if (IsAllUsers(topic))
            {
                await foreach (var userId in userStore.QueryIdsAsync(@event.AppId))
                {
                    yield return new Subscription
                    {
                        AppId = @event.AppId,
                        TopicPrefix = "users/all",
                        TopicSettings = null,
                        UserId = userId
                    };
                }
            }
            else if (IsUserTopic(topic, out var userId))
            {
                yield return new Subscription
                {
                    AppId = @event.AppId,
                    TopicPrefix = $"users/{userId}",
                    TopicSettings = null,
                    UserId = userId
                };
            }
            else
            {
                await foreach (var subscription in subscriptionStore.QueryAsync(@event.AppId, @event.Topic, @event.CreatorId))
                {
                    yield return subscription;
                }
            }
        }

        private static bool IsAllUsers(string topic)
        {
            return UserAllTopics.Contains(topic);
        }

        private static bool IsUserTopic(string topic, [MaybeNullWhen(false)] out string userId)
        {
            userId = null!;

            foreach (var prefix in UserTopicPrefixex)
            {
                if (topic.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    var afterPrefix = topic[prefix.Length..];

                    if (!string.IsNullOrWhiteSpace(afterPrefix))
                    {
                        userId = afterPrefix;

                        return true;
                    }
                }
            }

            return false;
        }

        private static UserEventMessage CreateUserEventMessage(EventMessage @event, Subscription subscription)
        {
            var result = SimpleMapper.Map(@event, new UserEventMessage
            {
                EventId = @event.Id,
                EventSettings = @event.Settings,
                SubscriptionPrefix = subscription.TopicPrefix,
                SubscriptionSettings = subscription.TopicSettings,
                Topic = @event.Topic,
                UserId = subscription.UserId
            });

            if (result.Properties == null)
            {
                result.Properties = new EventProperties();
            }

            if (result.SubscriptionSettings == null)
            {
                result.SubscriptionSettings = new NotificationSettings();
            }

            return result;
        }
    }
}
