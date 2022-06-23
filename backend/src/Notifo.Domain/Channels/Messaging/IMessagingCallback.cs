// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Channels.Messaging
{
    public record struct MessagingCallbackResponse(Guid NotificationId, MessagingResult Result, string? Details = null);

    public interface IMessagingCallback
    {
        Task HandleCallbackAsync(IMessagingSender sender, MessagingCallbackResponse response,
            CancellationToken ct);
    }
}
