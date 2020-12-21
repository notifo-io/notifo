// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Media.MongoDb
{
    public sealed class MongoDbMedia : MongoDbEntity<Media>
    {
        public static string CreateId(string appId, string fileName)
        {
            return $"{appId}_{fileName}";
        }

        public static MongoDbMedia FromMedia(Media media)
        {
            var id = CreateId(media.AppId, media.FileName);

            var result = new MongoDbMedia
            {
                DocId = id,
                Doc = media,
                Etag = GenerateEtag(),
            };

            return result;
        }

        public Media ToMedia()
        {
            return Doc;
        }
    }
}
