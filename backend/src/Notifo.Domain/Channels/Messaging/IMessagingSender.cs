// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Notifo.Domain.Apps;
using Notifo.Domain.Users;

namespace Notifo.Domain.Channels.Messaging
{
    public interface IMessagingSender
    {
        bool HasTarget(User user);

        Task AddTargetsAsync(MessagingJob job, User user);

        Task HandleCallbackAsync(App app, HttpContext httpContext);

        Task<bool> SendAsync(MessagingJob job, string text,
            CancellationToken ct);
    }
}
