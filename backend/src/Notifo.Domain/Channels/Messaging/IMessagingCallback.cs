// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Messaging
{
    public interface IMessagingCallback
    {
        Task HandleCallbackAsync(string to, Guid notificationId, MessagingResult result,
            CancellationToken ct);
    }
}
