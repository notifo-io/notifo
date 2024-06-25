// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Users;
using Notifo.Infrastructure.Collections;
using System.Collections.Generic;

namespace Notifo.Domain.Integrations;

public static class IntegrationExtensions
{
    public static UserInfo ToContext(this User user)
    {
        var properties = new Dictionary<string, string>(user.Properties);

        if (user.SystemProperties != null)
        {
            foreach (var (key, value) in user.SystemProperties)
            {
                properties[key] = value;
            }
        }

        return new UserInfo
        {
            Id = user.Id,
            EmailAddress = user.EmailAddress,
            PhoneNumber = user.PhoneNumber,
            Properties = properties.ToReadonlyDictionary()
        };
    }
}
