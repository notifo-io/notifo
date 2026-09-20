// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Integrations;

namespace Notifo.Domain.UserNotifications;

public sealed class EFTrackingBatch
{
    private readonly Dictionary<Guid, Change> changes = [];

    public sealed class Change(EFUserNotificationEntity entity)
    {
        public EFUserNotificationEntity Entity { get; } = entity;

        public UserNotification Notification { get; } = entity.ToNotification();

        public bool HasChanges { get; set; }

        public bool HasTrackingChanges { get; set; }

        public void MarkTracked()
        {
            HasChanges = true;
            HasTrackingChanges = true;
        }
    }

    public IEnumerable<Change> Changes => changes.Values;

    public EFTrackingBatch(IEnumerable<EFUserNotificationEntity> entities)
    {
        foreach (var entity in entities)
        {
            var change = new Change(entity);

            changes[change.Notification.Id] = change;
        }
    }

    public void UpdateStatus((TrackingToken Token, DeliveryResult Result)[] updates, Instant now)
    {
        foreach (var (token, result) in updates)
        {
            var (id, channel, _, _) = token;

            if (string.IsNullOrWhiteSpace(channel) || !changes.TryGetValue(id, out var change))
            {
                continue;
            }

            if (change.Notification.Channels.TryGetValue(channel, out var channelInfo) && TryGetConfiguration(channelInfo, token, out var configuration))
            {
                configuration.Status = result.Status;
                configuration.Detail = result.Detail;

                if (configuration.LastUpdate < now)
                {
                    configuration.LastUpdate = now;
                }

                change.MarkTracked();
            }
        }
    }

    public void MarkIfNotConfirmed(IEnumerable<TrackingToken> tokens, Instant now)
    {
        foreach (var token in tokens)
        {
            if (!changes.TryGetValue(token.UserNotificationId, out var change) || change.Notification.Formatting.ConfirmMode is not ConfirmMode.Explicit)
            {
                continue;
            }

            var notification = change.Notification;
            if (notification.FirstConfirmed == null)
            {
                notification.FirstConfirmed = new HandledInfo(now, token.Channel);
                change.MarkTracked();
            }

            // We only change the updated flag for notifications because otherwise the order could change with each tracking.
            if (notification.Updated < now)
            {
                notification.Updated = now;
                change.HasChanges = true;
            }

            MarkChannel(change, token, now, x => x.FirstConfirmed, (x, v) => x.FirstConfirmed = v, x => x.FirstConfirmed, (x, v) => x.FirstConfirmed = v);
        }
    }

    public void MarkIfNotSeen(IEnumerable<TrackingToken> tokens, Instant now)
    {
        foreach (var token in tokens)
        {
            if (!changes.TryGetValue(token.UserNotificationId, out var change))
            {
                continue;
            }

            if (change.Notification.FirstSeen == null)
            {
                change.Notification.FirstSeen = new HandledInfo(now, token.Channel);
                change.MarkTracked();
            }

            MarkChannel(change, token, now, x => x.FirstSeen, (x, v) => x.FirstSeen = v, x => x.FirstSeen, (x, v) => x.FirstSeen = v);
        }
    }

    public void MarkIfNotDelivered(IEnumerable<TrackingToken> tokens, Instant now)
    {
        foreach (var token in tokens)
        {
            if (!changes.TryGetValue(token.UserNotificationId, out var change))
            {
                continue;
            }

            if (change.Notification.FirstDelivered == null)
            {
                change.Notification.FirstDelivered = new HandledInfo(now, token.Channel);
                change.MarkTracked();
            }

            MarkChannel(change, token, now, x => x.FirstDelivered, (x, v) => x.FirstDelivered = v, x => x.FirstDelivered, (x, v) => x.FirstDelivered = v);
        }
    }

    private static void MarkChannel(Change change, TrackingToken token, Instant now,
        Func<UserNotificationChannel, Instant?> getChannel,
        Action<UserNotificationChannel, Instant> setChannel,
        Func<ChannelSendInfo, Instant?> getConfiguration,
        Action<ChannelSendInfo, Instant> setConfiguration)
    {
        if (string.IsNullOrWhiteSpace(token.Channel) || !change.Notification.Channels.TryGetValue(token.Channel, out var channelInfo))
        {
            return;
        }

        // The first timestamp is only written once, otherwise every tracking would report a change.
        if (getChannel(channelInfo) == null)
        {
            setChannel(channelInfo, now);
            change.MarkTracked();
        }

        if (TryGetConfiguration(channelInfo, token, out var configuration) && getConfiguration(configuration) == null)
        {
            setConfiguration(configuration, now);
            change.MarkTracked();
        }
    }

    private static bool TryGetConfiguration(UserNotificationChannel channel, TrackingToken token, out ChannelSendInfo configuration)
    {
        if (channel.Status.TryGetValue(token.ConfigurationId, out configuration!))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(token.Configuration))
        {
            foreach (var status in channel.Status.Values)
            {
                // If at least one of the configurations match to configuration string, we use this status.
                if (status.Configuration?.ContainsValue(token.Configuration!) == true)
                {
                    configuration = status;
                    return true;
                }
            }
        }

        return false;
    }
}
