// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Templates;

public sealed class MongoDbTemplate : MongoDbEntity<Template>
{
    public static string CreateId(string appId, string code)
    {
        return $"{appId}_{code}";
    }

    public static MongoDbTemplate FromTemplate(Template template)
    {
        var docId = CreateId(template.AppId, template.Code);

        var result = new MongoDbTemplate
        {
            DocId = docId,
            Doc = template,
            Etag = GenerateEtag()
        };

        return result;
    }

    public Template ToTemplate()
    {
        return Doc;
    }
}
