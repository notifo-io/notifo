﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Counters;
using Notifo.Domain.Events;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.Templates;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Reflection;
using IUserEventBus = Squidex.Messaging.IMessageBus;

namespace Notifo.Domain.UserEvents.Pipeline;

public sealed class UserEventPublisher(
    ICounterService counters,
    ILogStore logStore,
    IEventStore eventStore,
    ISubscriptionStore subscriptionStore,
    ITemplateStore templateStore,
    IUserStore userStore,
    IUserEventBus userEventProducer,
    ILogger<UserEventPublisher> log,
    Randomizer randomizer)
    : IUserEventPublisher
{
    private static readonly HashSet<string> UserAllTopics = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "users/all",
        "users/*",
        "user/all",
        "user/*"
    };

    private static readonly string[] UserTopicPrefixex =
    [
        "users/",
        "user/"
    ];

    public async Task PublishAsync(EventMessage @event,
        CancellationToken ct)
    {
        using (var activity = Telemetry.Activities.StartActivity("HandleUserEvent"))
        {
            log.LogInformation("Received event for app {appId} with ID {id} to topic {topic}.",
                @event.AppId,
                @event.Id,
                @event.Topic);

            if (string.IsNullOrWhiteSpace(@event.AppId))
            {
                log.LogInformation("Received invalid event with ID {id} to topic {topic}: No app id found.",
                    @event.Id,
                    @event.Topic);
                return;
            }

            if (string.IsNullOrWhiteSpace(@event.Topic))
            {
                await logStore.LogAsync(@event.AppId, LogMessage.Event_NoTopic("System"));
                return;
            }

            if (string.IsNullOrWhiteSpace(@event.TemplateCode) && @event.Formatting?.HasSubject() != true)
            {
                await logStore.LogAsync(@event.AppId, LogMessage.Event_NoSubjectOrTemplateCode("System"));
                return;
            }

            var count = 0;

            await foreach (var subscription in GetSubscriptions(@event, ct))
            {
                ct.ThrowIfCancellationRequested();

                if (count == 0)
                {
                    var templateCode = (string?)null;

                    if (@event.TemplateVariants?.Count > 0)
                    {
                        var random = randomizer.NextDouble();

                        var propability = 0d;

                        foreach (var (key, value) in @event.TemplateVariants)
                        {
                            propability += value;

                            if (random <= propability)
                            {
                                templateCode = key;
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(templateCode))
                    {
                        templateCode = @event.TemplateCode;
                    }

                    if (!string.IsNullOrWhiteSpace(templateCode))
                    {
                        var template = await templateStore.GetAsync(@event.AppId, templateCode, ct);

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

                            @event.Settings = ChannelSettings.Merged(template.Settings, @event.Settings);
                        }
                    }

                    if (@event.Formatting?.HasSubject() != true)
                    {
                        await logStore.LogAsync(@event.AppId, LogMessage.Event_NoTemplateSubject("System", templateCode!));
                        return;
                    }

                    if (@event.Properties != null)
                    {
                        @event.Formatting = @event.Formatting.Format(@event.Properties);
                    }

                    try
                    {
                        await eventStore.InsertAsync(@event, ct);
                    }
                    catch (UniqueConstraintException)
                    {
                        await logStore.LogAsync(@event.AppId, LogMessage.Event_AlreadyProcessed("System"));
                        break;
                    }
                }

                var userEventMessage = CreateUserEventMessage(@event, subscription);

                if (activity != null)
                {
                    userEventMessage.UserEventActivity = activity.Context;
                }

                await userEventProducer.PublishAsync(userEventMessage, subscription.UserId, default);
                count++;
            }

            if (count > 0)
            {
                var counterMap = CounterMap.ForNotification(DeliveryStatus.Attempt, count);
                var counterKey = TrackingKey.ForEvent(@event);

                await counters.CollectAsync(counterKey, counterMap, ct);
            }
            else
            {
                await logStore.LogAsync(@event.AppId, LogMessage.Event_NoSubscriber("System"));
            }

            log.LogInformation("Processed event for app {appId} with ID {id} to topic {topic}.",
                @event.AppId,
                @event.Id,
                @event.Topic);
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
        var result = new UserEventMessage
        {
            EventId = @event.Id
        };

        SimpleMapper.Map(@event, result);
        SimpleMapper.Map(subscription, result);

        if (subscription.TopicSettings != null)
        {
            result.Settings = ChannelSettings.Merged(@event.Settings, subscription.TopicSettings);
        }

        result.Scheduling = Scheduling.Merged(@event.Scheduling, subscription.Scheduling);

        return result;
    }
}
