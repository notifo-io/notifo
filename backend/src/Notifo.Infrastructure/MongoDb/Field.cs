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

            var classMap = BsonClassMap.LookupClassMap(typeof(T));

            while (classMap != null)
            {
                var field = classMap.GetMemberMap(name)?.ElementName;

                if (!string.IsNullOrWhiteSpace(field))
                {
                    return field;
                }

                classMap = classMap.BaseClassMap;
            }

            throw new InvalidOperationException("Cannot find member name.");
        }
    }
}
