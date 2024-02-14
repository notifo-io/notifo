// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;

namespace Notifo.Infrastructure.Collections.Bson;

public static class ReadonlyDictionarySerializer
{
    private static volatile int isRegistered;

    public static void Register()
    {
        if (Interlocked.Increment(ref isRegistered) == 1)
        {
            BsonSerializer.RegisterGenericSerializerDefinition(
                typeof(ReadonlyDictionary<,>),
                typeof(ReadonlyDictionarySerializer<,>));
        }
    }
}
