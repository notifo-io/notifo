// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Notifo.Domain.Apps;

namespace Notifo.Domain.Channels.Sms
{
    public interface ISmsSender
    {
        string Name { get; }

        Task<SmsResult> SendAsync(App app, string to, string body, string token,
            CancellationToken ct = default);

        Task HandleCallbackAsync(App app, HttpContext httpContext);
    }
}
