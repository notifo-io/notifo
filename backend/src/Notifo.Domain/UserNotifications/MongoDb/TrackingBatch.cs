// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Domain.UserNotifications.MongoDb;

public sealed class TrackingBatch
{
    private readonly Dictionary<Guid, Updates> pendingChanges = new Dictionary<Guid, Updates>();
    private readonly IMongoCollection<UserNotification> collection;

    private class Updates
    {
        public Instant Timestamp { get; set; }

        public UserNotification Notification { get; init; }

        public Dictionary<string, object?> Changes { get; } = new Dictionary<string, object?>();

        public void Add(Instant now, string key, object? value)
        {
            if (Timestamp < now)
            {
                Timestamp = now;
            }

            Changes[key] = value;
        }
    }

    public TrackingBatch(IMongoCollection<UserNotification> collection)
    {
        this.collection = collection;
    }

    private async Task ResolveNotificationsAsync(IEnumerable<TrackingToken> tokens,
        CancellationToken ct)
    {
        var notificationIds = tokens.Select(x => x.UserNotificationId).Distinct();
        var notificationItems = await collection.Find(x => notificationIds.Contains(x.Id)).ToListAsync(ct);

        foreach (var notification in notificationItems)
        {
            pendingChanges[notification.Id] = new Updates
            {
                Notification = notification
            };
        }
    }

    public UserNotification? GetNotification(Guid id)
    {
        return pendingChanges.GetOrAddDefault(id)?.Notification;
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
        if (pendingChanges.Count == 0)
        {
            return;
        }

        var writes = new List<WriteModel<UserNotification>>();

        foreach (var (id, updates) in pendingChanges)
        {
            if (updates.Changes.Count == 0)
            {
                continue;
            }

            if (updates.Timestamp != default)
            {
                updates.Changes["Updated"] = updates.Timestamp;
            }

            writes.Add(
                new UpdateOneModel<UserNotification>(
                    Builders<UserNotification>.Filter.Eq(x => x.Id, id),
                    Builders<UserNotification>.Update.Combine(
                        updates.Changes.Select(update => Builders<UserNotification>.Update.Set(update.Key, update.Value)))));
        }

        if (writes.Count == 0)
        {
            return;
        }

        await collection.BulkWriteAsync(writes, null, ct);
    }

    public void UpdateStatus((TrackingToken Token, ProcessStatus Status, string? Detail)[] updates, Instant now)
    {
        foreach (var (token, status, detail) in updates)
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
                    configuration.Status = status;
                    configuration.Detail = detail;

                    changes.Add(now, $"Channels.{channel}.Status.{configurationId}.Status", status);
                    changes.Add(now, $"Channels.{channel}.Status.{configurationId}.Detail", detail);
                    changes.Add(now, $"Channels.{channel}.Status.{configurationId}.LastUpdate", now);
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

                changes.Add(now, "FirstConfirmed.Timestamp", now);
                changes.Add(now, "FirstConfirmed.Channel", channel);
            }

            if (!string.IsNullOrWhiteSpace(channel) && notification.Channels.TryGetValue(channel, out var channelInfo))
            {
                if (ShouldUpdate(channelInfo.FirstConfirmed, now))
                {
                    channelInfo.FirstConfirmed = now;

                    changes.Add(now, $"Channels.{channel}.FirstConfirmed", now);
                }

                if (TryGetConfiguration(channelInfo, token, out var status, out var configurationId) && ShouldUpdate(status.FirstConfirmed, now))
                {
                    status.FirstConfirmed = now;

                    changes.Add(now, $"Channels.{channel}.Status.{configurationId}.FirstConfirmed", now);
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

                changes.Add(now, "FirstSeen.Timestamp", now);
                changes.Add(now, "FirstSeen.Channel", channel);
            }

            if (!string.IsNullOrWhiteSpace(channel) && notification.Channels.TryGetValue(channel, out var channelInfo))
            {
                if (ShouldUpdate(channelInfo.FirstSeen, now))
                {
                    channelInfo.FirstSeen = now;

                    changes.Add(now, $"Channels.{channel}.FirstSeen", now);
                }

                if (TryGetConfiguration(channelInfo, token, out var status, out var configurationId) && ShouldUpdate(status.FirstSeen, now))
                {
                    status.FirstSeen = now;

                    changes.Add(now, $"Channels.{channel}.Status.{configurationId}.FirstSeen", now);
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

                changes.Add(now, "FirstDelivered.Timestamp", now);
                changes.Add(now, "FirstDelivered.Channel", channel);
            }

            if (!string.IsNullOrWhiteSpace(channel) && notification.Channels.TryGetValue(channel, out var channelInfo))
            {
                if (ShouldUpdate(channelInfo.FirstDelivered, now))
                {
                    channelInfo.FirstDelivered = now;

                    changes.Add(now, $"Channels.{channel}.FirstDelivered", now);
                }

                if (TryGetConfiguration(channelInfo, token, out var status, out var configurationId) && ShouldUpdate(status.FirstDelivered, now))
                {
                    status.FirstDelivered = now;

                    changes.Add(now, $"Channels.{channel}.Status.{configurationId}.FirstDelivered", now);
                }
            }
        }
    }

    private static bool ShouldUpdate(Instant? current, Instant now)
    {
        return current == null || current > now;
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
