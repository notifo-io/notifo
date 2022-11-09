// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Channels.Sms;

public record struct SmsCallbackResponse(Guid NotificationId, string To, SmsResult Result, string? Details = null);

public interface ISmsCallback
{
    Task HandleCallbackAsync(ISmsSender sender, SmsCallbackResponse response,
        CancellationToken ct);
}
