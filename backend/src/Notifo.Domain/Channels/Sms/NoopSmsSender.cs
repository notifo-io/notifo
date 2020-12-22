// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Sms
{
    public sealed class NoopSmsSender : ISmsSender
    {
        public Task<SmsResult> SendAsync(string to, string body, string? token = null, CancellationToken ct = default)
        {
            throw new DomainException("SMS are not configured.");
        }

        public Task HandleStatusAsync(HttpContext httpContext)
        {
            return Task.CompletedTask;
        }

        public Task RegisterAsync(Func<string, SmsResult, Task> handler)
        {
            return Task.CompletedTask;
        }
    }
}
