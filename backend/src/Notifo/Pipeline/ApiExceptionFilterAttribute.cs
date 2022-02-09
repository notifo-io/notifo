// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Notifo.Areas.Api;
using Squidex.Log;

namespace Notifo.Pipeline
{
    public sealed class ApiExceptionFilterAttribute : ActionFilterAttribute, IExceptionFilter, IAsyncResultFilter
    {
        public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult { Value: ProblemDetails problem })
            {
                var (error, _) = problem.ToErrorDto(context.HttpContext);

                context.Result = GetResult(error);
            }

            return next();
        }

        public void OnException(ExceptionContext context)
        {
            var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<AppResources>>();

            var (error, unhandled) = context.Exception.ToErrorDto(localizer, context.HttpContext);

            if (unhandled != null)
            {
                var log = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiExceptionFilterAttribute>>();

                log.LogError(unhandled, "An unexpected exception has occurred.");
            }

            context.Result = GetResult(error);
        }

        private static IActionResult GetResult(ErrorDto error)
        {
            if (error.StatusCode == 404)
            {
                return new NotFoundResult();
            }

            return new ObjectResult(error)
            {
                StatusCode = error.StatusCode
            };
        }
    }
}
