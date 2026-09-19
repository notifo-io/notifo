// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Topics.MongoDb;

public sealed class MongoDbTopic : MongoDbEntity<Topic>
{
    public static string CreateId(string appId, string path)
    {
        return $"{appId}_{path}";
    }

    public static MongoDbTopic FromTopic(Topic topic)
    {
        var docId = CreateId(topic.AppId, topic.Path);

        var result = new MongoDbTopic
        {
            DocId = docId,
            Doc = topic,
            Etag = GenerateEtag()
        };

        return result;
    }

    public Topic ToTopic()
    {
        return Doc;
    }
}
