// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;

namespace Notifo.Domain.UserNotifications;

internal sealed class TrackingChange
{
    private readonly Dictionary<string, UpdateDefinition<UserNotification>> changes = [];

    public UserNotification Notification { get; init; }

    public bool HasChanges => changes.Count > 0;

    // The updated timestamp is only used for sorting and does not indicate a real change.
    public bool HasTrackingChanges { get; private set; }

    public void Min(string key, object? value)
    {
        if (value == null)
        {
            return;
        }

        changes[key] = Builders<UserNotification>.Update.Min(key, value);
        HasTrackingChanges = true;
    }

    public void Max(string key, object? value)
    {
        if (value == null)
        {
            return;
        }

        changes[key] = Builders<UserNotification>.Update.Max(key, value);
    }

    public void Set(string key, object? value)
    {
        if (value == null)
        {
            return;
        }

        changes[key] = Builders<UserNotification>.Update.Set(key, value);
        HasTrackingChanges = true;
    }

    public WriteModel<UserNotification>? ToWrite()
    {
        if (changes.Count == 0)
        {
            return null;
        }

        var filter = Builders<UserNotification>.Filter.Eq(x => x.Id, Notification.Id);

        return new UpdateOneModel<UserNotification>(filter,
            Builders<UserNotification>.Update.Combine(changes.Values));
    }
}
