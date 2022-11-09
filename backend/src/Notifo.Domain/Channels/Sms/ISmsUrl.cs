// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Sms;

public interface ISmsUrl : IUserNotificationUrl
{
    string SmsWebhookUrl(string appId, string integrationId, Dictionary<string, string>? query = null);
}
