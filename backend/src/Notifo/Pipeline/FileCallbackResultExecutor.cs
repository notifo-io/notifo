﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using Squidex.Assets;

namespace Notifo.Pipeline;

public sealed class FileCallbackResultExecutor(ILoggerFactory loggerFactory) : FileResultExecutorBase(CreateLogger<FileCallbackResultExecutor>(loggerFactory))
{
    public async Task ExecuteAsync(ActionContext context, FileCallbackResult result)
    {
        try
        {
            var (range, _, serveBody) = SetHeadersAndLog(context, result, result.FileSize, result.FileSize.HasValue);

            if (!string.IsNullOrWhiteSpace(result.FileDownloadName) && result.SendInline)
            {
                var headerValue = new ContentDispositionHeaderValue("inline");

                headerValue.SetHttpFileName(result.FileDownloadName);

                context.HttpContext.Response.Headers[HeaderNames.ContentDisposition] = headerValue.ToString();
            }

            if (serveBody)
            {
                var bytesRange = new BytesRange(range?.From, range?.To);

                await result.Callback(
                    context.HttpContext.Response.Body,
                    context.HttpContext,
                    bytesRange,
                    context.HttpContext.RequestAborted);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            if (!context.HttpContext.Response.HasStarted && result.ErrorAs404)
            {
                context.HttpContext.Response.Headers.Clear();
                context.HttpContext.Response.StatusCode = 404;

                Logger.LogCritical(new EventId(99), e, "Failed to send result.");
            }
            else
            {
                throw;
            }
        }
    }
}
