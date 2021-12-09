// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Notifo.Domain.Counters;
using Notifo.Domain.Events;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.Templates;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Reflection;
using Squidex.Log;
using IUserEventProducer = Notifo.Infrastructure.Messaging.IMessageProducer<Notifo.Domain.UserEvents.UserEventMessage>;

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

        private readonly ICounterService counters;
        private readonly IEventStore eventStore;
        private readonly ILogStore logStore;
        private readonly ISemanticLog log;
        private readonly ISubscriptionStore subscriptionStore;
        private readonly ITemplateStore templateStore;
        private readonly IUserEventProducer userEventProducer;
        private readonly IUserStore userStore;

        public UserEventPublisher(ICounterService counters, ILogStore logStore,
            IEventStore eventStore,
            ISubscriptionStore subscriptionStore,
            ITemplateStore templateStore,
            IUserStore userStore,
            IUserEventProducer userEventProducer,
            ISemanticLog log)
        {
            this.counters = counters;
            this.eventStore = eventStore;
            this.log = log;
            this.logStore = logStore;
            this.subscriptionStore = subscriptionStore;
            this.templateStore = templateStore;
            this.userEventProducer = userEventProducer;
            this.userStore = userStore;
        }

        public async Task PublishAsync(EventMessage @event,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("HandleUserEvent"))
            {
                log.LogInformation(@event, (m, w) => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("status", "Started")
                    .WriteProperty("appId", m.AppId)
                    .WriteProperty("eventId", m.Id)
                    .WriteProperty("eventTopic", m.Topic)
                    .WriteProperty("eventType", m.ToString()));

                if (string.IsNullOrWhiteSpace(@event.AppId))
                {
                    log.LogWarning(@event, (m, w) => w
                        .WriteProperty("action", "HandleEvent")
                        .WriteProperty("status", "Failed")
                        .WriteProperty("reason", "NoAppId")
                        .WriteProperty("appId", m.AppId)
                        .WriteProperty("eventId", m.Id)
                        .WriteProperty("eventTopic", m.Topic)
                        .WriteProperty("eventType", m.ToString()));
                    return;
                }

                if (string.IsNullOrWhiteSpace(@event.Topic))
                {
                    await logStore.LogAsync(@event.AppId, Texts.Events_NoTopic);
                    return;
                }

                if (string.IsNullOrWhiteSpace(@event.TemplateCode) && @event.Formatting?.HasSubject() != true)
                {
                    await logStore.LogAsync(@event.AppId, Texts.Events_NoSubjectOrTemplateCode);
                    return;
                }

                var count = 0;

                await foreach (var subscription in GetSubscriptions(@event, ct))
                {
                    ct.ThrowIfCancellationRequested();

                    if (count == 0)
                    {
                        if (!string.IsNullOrWhiteSpace(@event.TemplateCode))
                        {
                            var template = await templateStore.GetAsync(@event.AppId, @event.TemplateCode, ct);

                            if (template?.IsAutoCreated == false)
                            {
                                if (@event.Formatting != null)
                                {
                                    @event.Formatting = template.Formatting.MergedWith(@event.Formatting);
                                }
                                else
                                {
                                    @event.Formatting = template.Formatting;
                                }

                                if (template.Settings?.Count > 0)
                                {
                                    var settings = new NotificationSettings();

                                    settings.OverrideBy(template.Settings);
                                    settings.OverrideBy(@event.Settings);

                                    @event.Settings = settings;
                                }
                            }
                        }

                        if (@event.Formatting?.HasSubject() != true)
                        {
                            await logStore.LogAsync(@event.AppId, string.Format(CultureInfo.InvariantCulture, Texts.Template_NoSubject, @event.TemplateCode));
                            return;
                        }

                        @event.Formatting = @event.Formatting.Format(@event.Properties);

                        try
                        {
                            await eventStore.InsertAsync(@event, ct);
                        }
                        catch (UniqueConstraintException)
                        {
                            await logStore.LogAsync(@event.AppId, Texts.Events_AlreadyProcessed);
                            break;
                        }
                    }

                    var userEventMessage = CreateUserEventMessage(@event, subscription);

#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods that take one
#pragma warning disable MA0040 // Flow the cancellation token
                    await userEventProducer.ProduceAsync(subscription.UserId, userEventMessage);
#pragma warning restore MA0040 // Flow the cancellation token
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods that take one

                    count++;
                }

                if (count > 0)
                {
                    var counterMap = CounterMap.ForNotification(ProcessStatus.Attempt, count);
                    var counterKey = CounterKey.ForEvent(@event);

                    await counters.CollectAsync(counterKey, counterMap, ct);
                }
                else
                {
                    await logStore.LogAsync(@event.AppId, Texts.Events_NoSubscriber);
                }

                log.LogInformation(@event, (m, w) => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("status", "Success")
                    .WriteProperty("appId", m.AppId)
                    .WriteProperty("eventId", m.Id)
                    .WriteProperty("eventTopic", m.Topic)
                    .WriteProperty("eventType", m.ToString()));
            }
        }

        private async IAsyncEnumerable<Subscription> GetSubscriptions(EventMessage @event,
            [EnumeratorCancellation] CancellationToken ct)
        {
            var topic = @event.Topic;

            if (IsAllUsers(topic))
            {
                await foreach (var userId in userStore.QueryIdsAsync(@event.AppId, ct))
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
            else
            {
                await foreach (var subscription in subscriptionStore.QueryAsync(@event.AppId, @event.Topic, @event.CreatorId, ct))
                {
                    yield return subscription;
                }

                if (IsUserTopic(topic, out var userId)) 
                {
                    yield return new Subscription
                    {
                        AppId = @event.AppId,
                        TopicPrefix = $"users/{userId}",
                        TopicSettings = null,
                        UserId = userId
                    };
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
                result.Properties = new NotificationProperties();
            }

            if (result.SubscriptionSettings == null)
            {
                result.SubscriptionSettings = new NotificationSettings();
            }

            return result;
        }
    }
}
