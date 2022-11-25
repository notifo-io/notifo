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
    private Dictionary<string, UpdateDefinition<UserNotification>> changes = new Dictionary<string, UpdateDefinition<UserNotification>>();

    public UserNotification Notification { get; init; }

    public bool HasChanges => changes.Count > 0;

    public void Min(string key, object? value)
    {
        if (value == null)
        {
            return;
        }

        changes[key] = Builders<UserNotification>.Update.Min(key, value);
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
