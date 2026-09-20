// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Notifo.Infrastructure;

namespace Notifo.Domain.Subscriptions;

[Table("Subscriptions")]
[Index(nameof(AppId), nameof(TopicPrefix))]
[Index(nameof(AppId), nameof(UserId))]
public sealed class EFSubscriptionEntity : EFEntity<Subscription>
{
    [MaxLength(FieldLengths.AppId)]
    public string AppId { get; set; }

    [MaxLength(FieldLengths.Id)]
    public string UserId { get; set; }

    [MaxLength(FieldLengths.Key)]
    public string TopicPrefix { get; set; }

    public static string CreateId(string appId, string userId, string topicPrefix)
    {
        return $"{appId}_{userId}_{topicPrefix}";
    }

    public static EFSubscriptionEntity FromSubscription(Subscription subscription)
    {
        return new EFSubscriptionEntity
        {
            AppId = subscription.AppId,
            Doc = subscription,
            DocId = CreateId(subscription.AppId, subscription.UserId, subscription.TopicPrefix),
            Etag = GenerateEtag(),
            TopicPrefix = subscription.TopicPrefix,
            UserId = subscription.UserId,
        };
    }

    public Subscription ToSubscription()
    {
        return Doc;
    }
}
