// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;

namespace Notifo.Identity.MongoDb
{
    public sealed class MongoDbXmlEntity
    {
        [BsonId]
        public string FriendlyName { get; set; }

        [BsonRequired]
        public string Xml { get; set; }
    }
}
