// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;

namespace Notifo.Domain.Channels.Messaging;

public interface IMessagingCallback
{
    Task HandleCallbackAsync(IMessagingSender source, Guid notificationId, MessagingResult result, string? details = null);
}
