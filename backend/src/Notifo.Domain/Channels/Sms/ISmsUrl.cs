// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Sms;

public interface ISmsUrl
{
    string SmsWebhookUrl(string appId, string integrationId, Guid notificationId, string phoneNumber);
}
