// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;

namespace Notifo.Domain.Integrations.MessageBird.Implementation
{
    public interface IMessageBirdClient
    {
        Task<MessageBirdSmsStatus> ParseStatusAsync(HttpContext httpContext);

        Task<MessageBirdSmsResponse> SendSmsAsync(MessageBirdSmsMessage message, CancellationToken ct);
    }
}
