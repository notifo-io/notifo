// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Sms;

public interface ISmsCallback
{
    Task HandleCallbackAsync(string integrationId, string integrationName, Guid notificationId, string phoneNumber, SmsCallbackResponse response,
        CancellationToken ct);
}
