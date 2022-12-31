// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Users;

namespace Notifo.Domain.Integrations;

public static class IntegrationExtensions
{
    public static UserContext ToContext(this User user)
    {
        return new UserContext { Id = user.Id, EmailAddress = user.EmailAddress, PhoneNumber = user.PhoneNumber, Properties = user.SystemProperties };
    }

    public static AppContext ToContext(this App app)
    {
        return new AppContext { Id = app.Id, Name = app.Name };
    }
}
