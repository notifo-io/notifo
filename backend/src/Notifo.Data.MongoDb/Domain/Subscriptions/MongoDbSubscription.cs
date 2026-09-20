// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using Notifo.Infrastructure;

namespace Notifo.Domain.Subscriptions;

public sealed class MongoDbSubscription : MongoDbEntity
{
    [BsonRequired]
    [BsonElement("a")]
    public string AppId { get; set; }

    [BsonRequired]
    [BsonElement("u")]
    public string UserId { get; set; }

    [BsonRequired]
    [BsonElement("ta")]
    public string[] TopicArray { get; set; }

    [BsonRequired]
    [BsonElement("t")]
    public string TopicPrefix { get; set; }

    [BsonRequired]
    [BsonElement("s")]
    public ChannelSettings? TopicSettings { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("sc")]
    public Scheduling? Scheduling { get; set; }

    public static string CreateId(string appId, string userId, string topicPrefix)
    {
        return $"{appId}_{userId}_{topicPrefix}";
    }

    public static MongoDbSubscription FromSubscription(Subscription subscription)
    {
        var id = CreateId(subscription.AppId, subscription.UserId, subscription.TopicPrefix);

        var result = new MongoDbSubscription
        {
            DocId = id,
            AppId = subscription.AppId,
            TopicArray = subscription.TopicPrefix.GetParts(),
            TopicPrefix = subscription.TopicPrefix,
            TopicSettings = subscription.TopicSettings,
            Scheduling = subscription.Scheduling,
            UserId = subscription.UserId,
            Etag = GenerateEtag()
        };

        return result;
    }

    public Subscription ToSubscription()
    {
        return new Subscription
        {
            AppId = AppId,
            TopicPrefix = TopicPrefix,
            TopicSettings = TopicSettings,
            Scheduling = Scheduling,
            UserId = UserId
        };
    }
}
