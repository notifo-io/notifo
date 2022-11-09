// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.ChannelTemplates.MongoDb;

public sealed class MongoDbChannelTemplate<T> : MongoDbEntity<ChannelTemplate<T>>
{
    public static string CreateId(string appId, string id)
    {
        return $"{appId}_{id}";
    }

    public static MongoDbChannelTemplate<T> FromChannelTemplate(ChannelTemplate<T> template)
    {
        var docId = CreateId(template.AppId, template.Id);

        var result = new MongoDbChannelTemplate<T>
        {
            DocId = docId,
            Doc = template,
            Etag = GenerateEtag()
        };

        return result;
    }

    public ChannelTemplate<T> ToChannelTemplate()
    {
        return Doc;
    }
}
