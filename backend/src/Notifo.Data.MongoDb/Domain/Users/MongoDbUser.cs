// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.Users.MongoDb;

public sealed class MongoDbUser : MongoDbEntity<User>
{
    public static string CreateId(string appId, string id)
    {
        return $"{appId}_{id}";
    }

    public static MongoDbUser FromUser(User user)
    {
        var id = CreateId(user.AppId, user.Id);

        var result = new MongoDbUser
        {
            DocId = id,
            Doc = user,
            Etag = GenerateEtag()
        };

        return result;
    }

    public User ToUser()
    {
        return Doc;
    }
}
