// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Timers;
using Squidex.Hosting;

namespace Notifo.Domain.UserNotifications;

public sealed class EFUserNotificationRepository<TContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IOptions<UserNotificationsOptions> options,
    IClock clock,
    ILogger<EFUserNotificationRepository<TContext>> log)
    : EFStore<TContext, EFUserNotificationEntity, UserNotification>(dbContextFactory), IUserNotificationRepository, IInitializable where TContext : DbContext
{
    private const int MaxAttempts = 10;
    private readonly UserNotificationsOptions options = options.Value;
    private CompletionTimer? timer;

    public Task InitializeAsync(
        CancellationToken ct)
    {
        timer = new CompletionTimer((int)TimeSpan.FromMinutes(10).TotalMilliseconds, CleanupAsync);
        return Task.CompletedTask;
    }

    public Task ReleaseAsync(
        CancellationToken ct)
    {
        return timer?.StopAsync() ?? Task.CompletedTask;
    }

    public async Task CleanupAsync(
        CancellationToken ct)
    {
        var maxAge = clock.GetCurrentInstant().Minus(Duration.FromTimeSpan(options.RetentionTime));

        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFUserNotificationEntity>().Where(x => x.Created < maxAge)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<bool> IsHandledOrConfirmedAsync(Guid id, string channel, Guid configurationId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserNotificationRepository/IsHandledOrConfirmedAsync"))
        {
            return await IsAsync(id, x => x.FirstConfirmed != null || IsHandled(x, channel, configurationId), ct);
        }
    }

    public async Task<bool> IsHandledOrSeenAsync(Guid id, string channel, Guid configurationId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserNotificationRepository/IsHandledOrSeenAsync"))
        {
            return await IsAsync(id, x => x.FirstSeen != null || IsHandled(x, channel, configurationId), ct);
        }
    }

    public async Task<bool> IsHandledAsync(Guid id, string channel, Guid configurationId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserNotificationRepository/IsHandledAsync"))
        {
            return await IsAsync(id, x => IsHandled(x, channel, configurationId), ct);
        }
    }

    private async Task<bool> IsAsync(Guid id, Func<UserNotification, bool> predicate,
        CancellationToken ct)
    {
        var notification = await FindAsync(id, ct);

        return notification != null && predicate(notification);
    }

    private static bool IsHandled(UserNotification notification, string channel, Guid configurationId)
    {
        return
            notification.Channels.TryGetValue(channel, out var channelInfo) &&
            channelInfo.Status.TryGetValue(configurationId, out var status) &&
            status.Status is DeliveryStatus.Handled;
    }

    public async Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("EFUserNotificationRepository/QueryAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var filtered = Filter(dbContext.Set<EFUserNotificationEntity>().Where(x => x.AppId == appId && x.UserId == userId), query);

            // When the query continues from a timestamp, the results must be sorted by the same field.
            // Otherwise the continuation token would skip the notifications that do not fit on the page.
            var sorted =
                query.After != default ?
                filtered.OrderBy(x => x.Updated) :
                filtered.OrderByDescending(x => x.Created);

            return await filtered.ToResultListAsync(sorted, query, x => x.ToNotification(), activity, ct);
        }
    }

    public async Task<IResultList<UserNotification>> QueryAsync(string appId, UserNotificationQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("EFUserNotificationRepository/QueryAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var filtered = Filter(dbContext.Set<EFUserNotificationEntity>().Where(x => x.AppId == appId), query);

            return await filtered.ToResultListAsync(filtered.OrderByDescending(x => x.Created), query, x => x.ToNotification(), activity, ct);
        }
    }

    public async Task<IReadOnlyDictionary<string, Instant>> QueryLastNotificationsAsync(string appId, IEnumerable<string> userIds,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserNotificationRepository/QueryLastNotificationsAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var ids = userIds.ToList();

            var lastNotifications =
                await dbContext.Set<EFUserNotificationEntity>()
                    .Where(x => x.AppId == appId && ids.Contains(x.UserId) && !x.IsDeleted)
                    .GroupBy(x => x.UserId)
                    .Select(x => new { UserId = x.Key, Created = x.Max(y => y.Created) })
                    .ToListAsync(ct);

            return lastNotifications.ToDictionary(x => x.UserId, x => x.Created);
        }
    }

    public async Task<UserNotification?> FindAsync(Guid id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserNotificationRepository/FindAsync"))
        {
            var entity = await GetDocumentAsync(EFUserNotificationEntity.CreateId(id), ct);

            return entity?.ToNotification();
        }
    }

    public async Task DeleteAsync(Guid id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserNotificationRepository/DeleteAsync"))
        {
            var docId = EFUserNotificationEntity.CreateId(id);

            await using var dbContext = await CreateDbContextAsync(ct);

            await dbContext.Set<EFUserNotificationEntity>().Where(x => x.DocId == docId)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.IsDeleted, true), ct);
        }
    }

    public async Task InsertAsync(UserNotification notification,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserNotificationRepository/InsertAsync"))
        {
            var entity = EFUserNotificationEntity.FromNotification(notification);

            await InsertDocumentAsync(entity, ct);
            await CleanupAsync(notification, ct);
        }
    }

    private async Task CleanupAsync(UserNotification notification,
        CancellationToken ct)
    {
        if (options.MaxItemsPerUser is <= 0 or >= int.MaxValue)
        {
            return;
        }

        using (Telemetry.Activities.StartActivity("EFUserNotificationRepository/CleanupAsync"))
        {
            try
            {
                await using var dbContext = await CreateDbContextAsync(ct);

                // Sort descending because we do not want to delete the newest elements.
                var oldIds =
                    await dbContext.Set<EFUserNotificationEntity>()
                        .Where(x => x.AppId == notification.AppId && x.UserId == notification.UserId)
                        .OrderByDescending(x => x.Created)
                        .Skip(options.MaxItemsPerUser)
                        .Select(x => x.DocId)
                        .ToListAsync(ct);

                foreach (var batch in oldIds.Chunk(1000))
                {
                    await dbContext.Set<EFUserNotificationEntity>().Where(x => batch.Contains(x.DocId))
                        .ExecuteDeleteAsync(ct);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.LogError(ex, "Failed to cleanup notifications.");
            }
        }
    }

    public async Task BatchWriteAsync((TrackingToken Token, DeliveryResult Result)[] updates, Instant now,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserNotificationRepository/BatchWriteAsync"))
        {
            await TrackAsync(updates.Select(x => x.Token), batch => batch.UpdateStatus(updates, now), ct);
        }
    }

    public async Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackConfirmedAsync(TrackingToken[] tokens, Instant now,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserNotificationRepository/TrackConfirmedAsync"))
        {
            return await TrackAsync(tokens, batch =>
            {
                batch.MarkIfNotConfirmed(tokens, now);
                batch.MarkIfNotSeen(tokens, now);
                batch.MarkIfNotDelivered(tokens, now);
            }, ct);
        }
    }

    public async Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackSeenAsync(TrackingToken[] tokens, Instant now,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserNotificationRepository/TrackSeenAsync"))
        {
            return await TrackAsync(tokens, batch =>
            {
                batch.MarkIfNotSeen(tokens, now);
                batch.MarkIfNotDelivered(tokens, now);
            }, ct);
        }
    }

    public async Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackDeliveredAsync(TrackingToken[] tokens, Instant now,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFUserNotificationRepository/TrackDeliveredAsync"))
        {
            return await TrackAsync(tokens, batch =>
            {
                batch.MarkIfNotDelivered(tokens, now);
            }, ct);
        }
    }

    private async Task<IReadOnlyList<(UserNotification, bool Updated)>> TrackAsync(IEnumerable<TrackingToken> tokens, Action<EFTrackingBatch> apply,
        CancellationToken ct)
    {
        var result = new List<(UserNotification, bool Updated)>();

        await using var dbContext = await CreateDbContextAsync(ct);

        var pendingIds = tokens.Select(x => EFUserNotificationEntity.CreateId(x.UserNotificationId)).ToHashSet();

        // The changes are applied to the whole document, so we have to retry when it has been changed in the meantime.
        for (var attempt = 0; attempt < MaxAttempts && pendingIds.Count > 0; attempt++)
        {
            var entities =
                await dbContext.Set<EFUserNotificationEntity>()
                    .Where(x => pendingIds.Contains(x.DocId))
                    .ToListAsync(ct);

            var batch = new EFTrackingBatch(entities);

            apply(batch);

            pendingIds.Clear();

            foreach (var change in batch.Changes)
            {
                if (change.HasChanges && !await TryWriteAsync(dbContext, change, ct))
                {
                    pendingIds.Add(change.Entity.DocId);
                    continue;
                }

                result.Add((change.Notification, change.HasTrackingChanges));
            }
        }

        if (pendingIds.Count > 0)
        {
            throw new InconsistentStateException(string.Empty, string.Empty);
        }

        return result;
    }

    private static async Task<bool> TryWriteAsync(TContext dbContext, EFTrackingBatch.Change change,
        CancellationToken ct)
    {
        var oldEtag = change.Entity.Etag;
        var newEtag = EFEntity<UserNotification>.GenerateEtag();

        var updated =
            await dbContext.Set<EFUserNotificationEntity>()
                .Where(x => x.DocId == change.Entity.DocId && x.Etag == oldEtag)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.Doc, change.Notification)
                    .SetProperty(x => x.Etag, newEtag)
                    .SetProperty(x => x.Updated, change.Notification.Updated),
                    ct);

        return updated == 1;
    }

    private static IQueryable<EFUserNotificationEntity> Filter(IQueryable<EFUserNotificationEntity> source, UserNotificationQuery query)
    {
        var after = query.After;

        source = source.Where(x => x.Updated >= after);

        switch (query.Scope)
        {
            case UserNotificationQueryScope.Deleted:
                source = source.Where(x => x.IsDeleted);
                break;
            case UserNotificationQueryScope.NonDeleted:
                source = source.Where(x => !x.IsDeleted);
                break;
        }

        if (!string.IsNullOrWhiteSpace(query.CorrelationId))
        {
            var correlationId = query.CorrelationId.ToMaxLength(FieldLengths.Id);

            source = source.Where(x => x.CorrelationId == correlationId);
        }

        source = source.WhereContainsIgnoreCase(query.Query, x => x.Subject);

        source = source.WhereContainsAnyTag(x => x.SendChannels, query.Channels);

        return source;
    }
}
