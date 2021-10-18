// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Apps;

namespace Notifo.Domain.Channels.Sms
{
    public interface ISmsSender
    {
        Task<SmsResult> SendAsync(App app, string to, string body, string reference,
            CancellationToken ct = default);

        Task HandleCallbackAsync(App app, HttpContext httpContext);
    }
}
