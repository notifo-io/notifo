// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;

namespace Notifo.Domain.Subscriptions.MongoDb
{
    public sealed class MongoDbSubscription
    {
        [BsonId]
        public string DocId { get; set; }

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
        public NotificationSettings TopicSettings { get; set; }

        public static string CreateId(string appId, string userId, string topicPrefix)
        {
            return $"{appId}_{userId}_{topicPrefix}";
        }

        public static MongoDbSubscription FromSubscription(string appId, SubscriptionUpdate update)
        {
            var docId = CreateId(appId, update.UserId, update.TopicPrefix);

            var result = new MongoDbSubscription
            {
                DocId = docId,
                AppId = appId,
                TopicArray = new TopicId(update.TopicPrefix).GetParts(),
                TopicPrefix = update.TopicPrefix,
                TopicSettings = update.TopicSettings,
                UserId = update.UserId
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
                UserId = UserId
            };
        }
    }
}
