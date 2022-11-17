// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;

namespace Notifo.Domain.UserNotifications.MongoDb;

internal sealed class TrackingChange
{
    private Dictionary<string, (object Value, bool Max)> changes = new Dictionary<string, (object Value, bool Max)>();

    public UserNotification Notification { get; init; }

    public bool HasChanges => changes.Count > 0;

    public void Min(string key, object? value)
    {
        if (value == null)
        {
            return;
        }

        changes[key] = (value, false);
    }

    public void Max(string key, object? value)
    {
        if (value == null)
        {
            return;
        }

        changes[key] = (value, true);
    }

    public WriteModel<UserNotification>? ToWrite()
    {
        if (changes.Count == 0)
        {
            return null;
        }

        var update =
            Builders<UserNotification>.Update.Combine(
                changes.Select(change =>
                        change.Value.Max ?
                        Builders<UserNotification>.Update.Max(change.Key, change.Value.Value) :
                        Builders<UserNotification>.Update.Min(change.Key, change.Value.Value)));

        var filter = Builders<UserNotification>.Filter.Eq(x => x.Id, Notification.Id);

        return new UpdateOneModel<UserNotification>(filter, update);
    }
}
