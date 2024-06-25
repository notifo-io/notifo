// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Users;
using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.Integrations;

public static class IntegrationExtensions
{
    public static UserInfo ToContext(this User user)
    {
        return new UserInfo
        {
            Id = user.Id,
            EmailAddress = user.EmailAddress,
            PhoneNumber = user.PhoneNumber,
            Properties = user.Properties
            .Concat(
                user.SystemProperties ?? Enumerable.Empty<KeyValuePair<string, string>>()
            )
            .GroupBy(x => x.Key)
            .ToReadonlyDictionary(p => p.Key, p => p.Last().Value)
        };
    }
}
