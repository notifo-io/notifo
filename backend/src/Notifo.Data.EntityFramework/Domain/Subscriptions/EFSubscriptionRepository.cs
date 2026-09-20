// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Notifo.Infrastructure;

namespace Notifo.Domain.Subscriptions;

public sealed class EFSubscriptionRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : EFStore<TContext, EFSubscriptionEntity, Subscription>(dbContextFactory), ISubscriptionRepository where TContext : DbContext
{
    public async Task<IResultList<Subscription>> QueryAsync(string appId, SubscriptionQuery query,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("EFSubscriptionRepository/QueryAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var filtered = dbContext.Set<EFSubscriptionEntity>().Where(x => x.AppId == appId);

            if (!string.IsNullOrWhiteSpace(query.UserId))
            {
                filtered = filtered.Where(x => x.UserId == query.UserId);
            }

            if (query.Topics != null)
            {
                var topics = query.Topics;

                filtered = filtered.Where(x => topics.Contains(x.TopicPrefix));
            }
            else if (!string.IsNullOrWhiteSpace(query.Query))
            {
                filtered = filtered.WhereContainsIgnoreCase(query.Query, x => x.TopicPrefix);
            }

            return await filtered.ToResultListAsync(filtered.OrderBy(x => x.DocId), query, x => x.ToSubscription(), activity, ct);
        }
    }

    public async IAsyncEnumerable<Subscription> QueryAsync(string appId, TopicId topic, string? userId = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSubscriptionRepository/QueryAsyncByTopic"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var prefixes = GetPrefixes(topic);

            var query =
                dbContext.Set<EFSubscriptionEntity>()
                    .Where(x => x.AppId == appId && prefixes.Contains(x.TopicPrefix));

            if (userId != null)
            {
                query = query.Where(x => x.UserId != userId);
            }

            var lastSubscription = (EFSubscriptionEntity?)null;

            await foreach (var subscription in query.OrderBy(x => x.UserId).AsAsyncEnumerable().WithCancellation(ct))
            {
                if (!topic.Id.StartsWith(subscription.TopicPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Only return the most concrete subscription per user.
                if (string.Equals(subscription.UserId, lastSubscription?.UserId, StringComparison.OrdinalIgnoreCase))
                {
                    if (subscription.TopicPrefix.Length > lastSubscription!.TopicPrefix.Length)
                    {
                        lastSubscription = subscription;
                    }
                }
                else
                {
                    if (lastSubscription != null)
                    {
                        yield return lastSubscription.ToSubscription();
                    }

                    lastSubscription = subscription;
                }
            }

            if (lastSubscription != null)
            {
                yield return lastSubscription.ToSubscription();
            }
        }
    }

    public async Task<(Subscription? Subscription, string? Etag)> GetAsync(string appId, string userId, TopicId prefix,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSubscriptionRepository/GetAsync"))
        {
            var entity = await GetDocumentAsync(EFSubscriptionEntity.CreateId(appId, userId, prefix), ct);

            return (entity?.ToSubscription(), entity?.Etag);
        }
    }

    public async Task UpsertAsync(Subscription subscription, string? oldEtag = null,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSubscriptionRepository/UpsertAsync"))
        {
            await UpsertDocumentAsync(EFSubscriptionEntity.FromSubscription(subscription), oldEtag, ct);
        }
    }

    public async Task DeleteAsync(string appId, string userId, TopicId topic,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSubscriptionRepository/DeleteAsync"))
        {
            await DeleteDocumentAsync(EFSubscriptionEntity.CreateId(appId, userId, topic), ct);
        }
    }

    public async Task DeletePrefixAsync(string appId, string userId, TopicId prefix,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("EFSubscriptionRepository/DeletePrefixAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            // Delete the parent subscriptions, the subscription itself and all children.
            var prefixes = GetPrefixes(prefix);
            var children = $"{prefix.Id}/";

            await dbContext.Set<EFSubscriptionEntity>()
                .Where(x => x.AppId == appId && x.UserId == userId)
#pragma warning disable RECS0063 // StartsWith is translated to SQL
                .Where(x => prefixes.Contains(x.TopicPrefix) || x.TopicPrefix.StartsWith(children))
#pragma warning restore RECS0063
                .ExecuteDeleteAsync(ct);
        }
    }

    private static List<string> GetPrefixes(TopicId topic)
    {
        var parts = topic.GetParts();
        var result = new List<string>(parts.Length);

        for (var i = 1; i <= parts.Length; i++)
        {
            result.Add(string.Join('/', parts, 0, i));
        }

        return result;
    }
}
