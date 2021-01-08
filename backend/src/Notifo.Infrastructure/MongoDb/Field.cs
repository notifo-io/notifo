// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization;

namespace Notifo.Infrastructure.MongoDb
{
    public static class Field
    {
        public static string Of<T>(Func<T, string> mapper)
        {
            var name = mapper(default!);

            return BsonClassMap.LookupClassMap(typeof(T)).GetMemberMap(name).ElementName;
        }
    }
}
