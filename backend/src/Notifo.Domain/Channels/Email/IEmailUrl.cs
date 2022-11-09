// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Email;

public interface IEmailUrl : IUserNotificationUrl
{
    string EmailPreferences(string apiKey, string language);
}
