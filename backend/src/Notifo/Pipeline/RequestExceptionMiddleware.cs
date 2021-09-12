// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Notifo.Areas.Api;
using Squidex.Log;

namespace Notifo.Pipeline
{
    public sealed class RequestExceptionMiddleware
    {
        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();
        private static readonly RouteData EmptyRouteData = new RouteData();
        private readonly RequestDelegate next;

        public RequestExceptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, IActionResultExecutor<ObjectResult> writer, ISemanticLog log)
        {
            if (context.Request.Query.TryGetValue("error", out var header) && int.TryParse(header, NumberStyles.Integer, CultureInfo.InvariantCulture, out var statusCode) && IsErrorStatusCode(statusCode))
            {
                var (error, _) = ApiExceptionConverter.ToErrorDto(statusCode, context);

                await WriteErrorAsync(context, error, writer);
                return;
            }

            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w.WriteProperty("message", "An unexpected exception has occurred."));

                if (!context.Response.HasStarted)
                {
                    var localizer = context.RequestServices.GetRequiredService<IStringLocalizer<AppResources>>();

                    var (error, _) = ex.ToErrorDto(localizer, context);

                    await WriteErrorAsync(context, error, writer);
                }
            }

            if (IsErrorStatusCode(context.Response.StatusCode) && !context.Response.HasStarted)
            {
                var (error, _) = ApiExceptionConverter.ToErrorDto(context.Response.StatusCode, context);

                await WriteErrorAsync(context, error, writer);
            }
        }

        private static async Task WriteErrorAsync(HttpContext context, ErrorDto error, IActionResultExecutor<ObjectResult> writer)
        {
            var actionRouteData = context.GetRouteData() ?? EmptyRouteData;
            var actionContext = new ActionContext(context, actionRouteData, EmptyActionDescriptor);

            await writer.ExecuteAsync(actionContext, new ObjectResult(error)
            {
                StatusCode = error.StatusCode
            });
        }

        private static bool IsErrorStatusCode(int statusCode)
        {
            return statusCode is >= 400 and < 600;
        }
    }
}
