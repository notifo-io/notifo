// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using NodaTime;

namespace Notifo.Domain.UserNotifications.MongoDb;

public sealed class TrackingBatch
{
    private readonly Dictionary<Guid, Updates> pendingChanges = new Dictionary<Guid, Updates>();
    private readonly IMongoCollection<UserNotification> collection;

    private class Updates
    {
        public UserNotification Notification { get; init; }

        public Dictionary<string, object?> Changes { get; } = new Dictionary<string, object?>();

        public void Add(string key, object? value)
        {
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

    public List<(UserNotification, bool Updated)> GetNotifications()
    {
        return pendingChanges.Select(x => (x.Value.Notification, x.Value.Changes.Count > 0)).ToList();
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

                    changes.Add($"Channels.{channel}.Status.{configurationId}.Status", status);
                    changes.Add($"Channels.{channel}.Status.{configurationId}.Detail", detail);
                    changes.Add($"Channels.{channel}.Status.{configurationId}.LastUpdate", now);
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

                changes.Add("FirstConfirmed.Timestamp", now);
                changes.Add("FirstConfirmed.Channel", channel);
            }

            // We only change the updated flag for notifications because otherwise the order could change with each tracking.
            if (ShouldUpdate(notification.Updated, now))
            {
                notification.Updated = now;

                changes.Add("Updated", now);
            }

            if (!string.IsNullOrWhiteSpace(channel) && notification.Channels.TryGetValue(channel, out var channelInfo))
            {
                if (ShouldUpdate(channelInfo.FirstConfirmed, now))
                {
                    channelInfo.FirstConfirmed = now;

                    changes.Add($"Channels.{channel}.FirstConfirmed", now);
                }

                if (TryGetConfiguration(channelInfo, token, out var status, out var configurationId) && ShouldUpdate(status.FirstConfirmed, now))
                {
                    status.FirstConfirmed = now;

                    // Status value has been converted from old value.
                    if (status.Configuration.ContainsKey(Constants.Convertedkey))
                    {
                        if (!string.IsNullOrWhiteSpace(token.Configuration))
                        {
                            changes.Add($"Channels.{channel}.Status.{token.Configuration}.FirstConfirmed", now);
                        }
                    }
                    else
                    {
                        changes.Add($"Channels.{channel}.Status.{configurationId}.FirstConfirmed", now);
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

                changes.Add("FirstSeen.Timestamp", now);
                changes.Add("FirstSeen.Channel", channel);
            }

            if (!string.IsNullOrWhiteSpace(channel) && notification.Channels.TryGetValue(channel, out var channelInfo))
            {
                if (ShouldUpdate(channelInfo.FirstSeen, now))
                {
                    channelInfo.FirstSeen = now;

                    changes.Add($"Channels.{channel}.FirstSeen", now);
                }

                if (TryGetConfiguration(channelInfo, token, out var status, out var configurationId) && ShouldUpdate(status.FirstSeen, now))
                {
                    status.FirstSeen = now;

                    // Status value has been converted from old value.
                    if (status.Configuration.ContainsKey(Constants.Convertedkey))
                    {
                        if (!string.IsNullOrWhiteSpace(token.Configuration))
                        {
                            changes.Add($"Channels.{channel}.Status.{token.Configuration}.FirstSeen", now);
                        }
                    }
                    else
                    {
                        changes.Add($"Channels.{channel}.Status.{configurationId}.FirstSeen", now);
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

                changes.Add("FirstDelivered.Timestamp", now);
                changes.Add("FirstDelivered.Channel", channel);
            }

            if (!string.IsNullOrWhiteSpace(channel) && notification.Channels.TryGetValue(channel, out var channelInfo))
            {
                if (ShouldUpdate(channelInfo.FirstDelivered, now))
                {
                    channelInfo.FirstDelivered = now;

                    changes.Add($"Channels.{channel}.FirstDelivered", now);
                }

                if (TryGetConfiguration(channelInfo, token, out var status, out var configurationId) && ShouldUpdate(status.FirstDelivered, now))
                {
                    status.FirstDelivered = now;

                    // Status value has been converted from old value.
                    if (status.Configuration.ContainsKey(Constants.Convertedkey))
                    {
                        if (!string.IsNullOrWhiteSpace(token.Configuration))
                        {
                            changes.Add($"Channels.{channel}.Status.{token.Configuration}.FirstDelivered", now);
                        }
                    }
                    else
                    {
                        changes.Add($"Channels.{channel}.Status.{configurationId}.FirstDelivered", now);
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
                if (status.Configuration.ContainsValue(token.Configuration!) == true)
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
