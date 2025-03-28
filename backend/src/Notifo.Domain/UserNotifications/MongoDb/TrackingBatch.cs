﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using NodaTime;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications.MongoDb;

public sealed class TrackingBatch(IMongoCollection<UserNotification> collection)
{
    private readonly Dictionary<Guid, TrackingChange> pendingChanges = [];

    private async Task ResolveNotificationsAsync(IEnumerable<TrackingToken> tokens,
        CancellationToken ct)
    {
        var notificationIds = tokens.Select(x => x.UserNotificationId).Distinct();
        var notificationItems = await collection.Find(x => notificationIds.Contains(x.Id)).ToListAsync(ct);

        foreach (var notification in notificationItems)
        {
            pendingChanges[notification.Id] = new TrackingChange
            {
                Notification = notification
            };
        }
    }

    public List<(UserNotification, bool Updated)> GetNotifications()
    {
        return pendingChanges.Select(x => (x.Value.Notification, x.Value.HasChanges)).ToList();
    }

    public static async Task<TrackingBatch> CreateAsync(IMongoCollection<UserNotification> collection, IEnumerable<TrackingToken> tokens,
        CancellationToken ct)
    {
        var batch = new TrackingBatch(collection);

        await batch.ResolveNotificationsAsync(tokens, ct);

        return batch;
    }

    public async Task WriteAsync(
        CancellationToken ct)
    {
        if (pendingChanges.Count == 0 || !pendingChanges.Any(x => x.Value.HasChanges))
        {
            return;
        }

        var writes = pendingChanges.Select(x => x.Value.ToWrite()).NotNull().ToList();

        if (writes.Count == 0)
        {
            return;
        }

        await collection.BulkWriteAsync(writes, null, ct);
    }

    public void UpdateStatus((TrackingToken Token, DeliveryResult Result)[] updates, Instant now)
    {
        foreach (var (token, result) in updates)
        {
            var (id, channel, _, _) = token;

            if (string.IsNullOrWhiteSpace(channel))
            {
                continue;
            }

            if (!pendingChanges.TryGetValue(id, out var changes))
            {
                continue;
            }

            var notification = changes.Notification;

            if (notification.Channels.TryGetValue(channel, out var channelInfo))
            {
                if (TryGetConfiguration(channelInfo, token, out var configuration, out var configurationId))
                {
                    configuration.Status = result.Status;
                    configuration.Detail = result.Detail;

                    changes.Set($"Channels.{channel}.Status.{configurationId}.Status", result.Status);
                    changes.Set($"Channels.{channel}.Status.{configurationId}.Detail", result.Detail);
                    changes.Max($"Channels.{channel}.Status.{configurationId}.LastUpdate", now);
                }
            }
        }
    }

    public void MarkIfNotConfirmed(IEnumerable<TrackingToken> tokens, Instant now)
    {
        foreach (var token in tokens)
        {
            var (id, channel, _, _) = token;

            if (!pendingChanges.TryGetValue(id, out var changes) || changes.Notification.Formatting.ConfirmMode != ConfirmMode.Explicit)
            {
                continue;
            }

            var notification = changes.Notification;

            if (notification.FirstConfirmed == null)
            {
                notification.FirstConfirmed = new HandledInfo(now, channel);

                changes.Set("FirstConfirmed.Timestamp", now);
                changes.Set("FirstConfirmed.Channel", channel);
            }

            // We only change the updated flag for notifications because otherwise the order could change with each tracking.
            if (ShouldUpdate(notification.Updated, now))
            {
                notification.Updated = now;

                changes.Max("Updated", now);
            }

            if (!string.IsNullOrWhiteSpace(channel) && notification.Channels.TryGetValue(channel, out var channelInfo))
            {
                if (ShouldUpdate(channelInfo.FirstConfirmed, now))
                {
                    channelInfo.FirstConfirmed = now;

                    changes.Min($"Channels.{channel}.FirstConfirmed", now);
                }

                if (TryGetConfiguration(channelInfo, token, out var status, out var configurationId) && ShouldUpdate(status.FirstConfirmed, now))
                {
                    status.FirstConfirmed = now;

                    // Status value has been converted from old value.
                    if (status.Configuration.ContainsKey(Constants.Convertedkey))
                    {
                        if (!string.IsNullOrWhiteSpace(token.Configuration))
                        {
                            changes.Min($"Channels.{channel}.Status.{token.Configuration}.FirstConfirmed", now);
                        }
                    }
                    else
                    {
                        changes.Min($"Channels.{channel}.Status.{configurationId}.FirstConfirmed", now);
                    }
                }
            }
        }
    }

    public void MarkIfNotSeen(IEnumerable<TrackingToken> tokens, Instant now)
    {
        foreach (var token in tokens)
        {
            var (id, channel, _, _) = token;

            if (!pendingChanges.TryGetValue(id, out var changes))
            {
                continue;
            }

            var notification = changes.Notification;

            if (notification.FirstSeen == null)
            {
                notification.FirstSeen = new HandledInfo(now, channel);

                changes.Set("FirstSeen.Timestamp", now);
                changes.Set("FirstSeen.Channel", channel);
            }

            if (!string.IsNullOrWhiteSpace(channel) && notification.Channels.TryGetValue(channel, out var channelInfo))
            {
                if (ShouldUpdate(channelInfo.FirstSeen, now))
                {
                    channelInfo.FirstSeen = now;

                    changes.Min($"Channels.{channel}.FirstSeen", now);
                }

                if (TryGetConfiguration(channelInfo, token, out var status, out var configurationId) && ShouldUpdate(status.FirstSeen, now))
                {
                    status.FirstSeen = now;

                    // Status value has been converted from old value.
                    if (status.Configuration.ContainsKey(Constants.Convertedkey))
                    {
                        if (!string.IsNullOrWhiteSpace(token.Configuration))
                        {
                            changes.Min($"Channels.{channel}.Status.{token.Configuration}.FirstSeen", now);
                        }
                    }
                    else
                    {
                        changes.Min($"Channels.{channel}.Status.{configurationId}.FirstSeen", now);
                    }
                }
            }
        }
    }

    public void MarkIfNotDelivered(IEnumerable<TrackingToken> tokens, Instant now)
    {
        foreach (var token in tokens)
        {
            var (id, channel, _, _) = token;

            if (!pendingChanges.TryGetValue(id, out var changes))
            {
                continue;
            }

            var notification = changes.Notification;

            if (notification.FirstDelivered == null)
            {
                notification.FirstDelivered = new HandledInfo(now, channel);

                changes.Set("FirstDelivered.Timestamp", now);
                changes.Set("FirstDelivered.Channel", channel);
            }

            if (!string.IsNullOrWhiteSpace(channel) && notification.Channels.TryGetValue(channel, out var channelInfo))
            {
                if (ShouldUpdate(channelInfo.FirstDelivered, now))
                {
                    channelInfo.FirstDelivered = now;

                    changes.Min($"Channels.{channel}.FirstDelivered", now);
                }

                if (TryGetConfiguration(channelInfo, token, out var status, out var configurationId) && ShouldUpdate(status.FirstDelivered, now))
                {
                    status.FirstDelivered = now;

                    // Status value has been converted from old value.
                    if (status.Configuration.ContainsKey(Constants.Convertedkey))
                    {
                        if (!string.IsNullOrWhiteSpace(token.Configuration))
                        {
                            changes.Min($"Channels.{channel}.Status.{token.Configuration}.FirstDelivered", now);
                        }
                    }
                    else
                    {
                        changes.Min($"Channels.{channel}.Status.{configurationId}.FirstDelivered", now);
                    }
                }
            }
        }
    }

    private static bool ShouldUpdate(Instant? current, Instant now)
    {
        return current == null || current < now;
    }

    private static bool TryGetConfiguration(UserNotificationChannel channel, TrackingToken token, out ChannelSendInfo configuration, out Guid configurationId)
    {
        configurationId = default;

        if (channel.Status.TryGetValue(token.ConfigurationId, out configuration!))
        {
            configurationId = token.ConfigurationId;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(token.Configuration))
        {
            foreach (var (key, status) in channel.Status)
            {
                // If at least one of the configurations match to configuration string, we use the configuration ID of this status.
                if (status.Configuration?.ContainsValue(token.Configuration!) == true)
                {
                    configuration = status;
                    configurationId = key;
                    return true;
                }
            }
        }

        return false;
    }
}
