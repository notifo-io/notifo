// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Sms
{
    public interface ISmsCallback
    {
        Task HandleCallbackAsync(string to, Guid notificationId, SmsResult result,
            CancellationToken ct);
    }
}
