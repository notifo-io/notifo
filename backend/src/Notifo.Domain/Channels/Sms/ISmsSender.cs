// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Channels.Sms;

public interface ISmsSender
{
    string Name { get; }

    Task<SmsResult> SendAsync(SmsRequest message,
        CancellationToken ct = default);

    ValueTask<SmsCallbackResponse> HandleCallbackAsync(HttpContext httpContext);
}

public enum SmsResult
{
    Unknown,
    Skipped,
    Sent,
    Delivered,
    Failed
}

public record struct SmsRequest(string To, string Message, string? CallbackUrl = null);

public record struct SmsCallbackResponse(SmsResult Result, string? Details = null);
