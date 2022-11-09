// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Notifo.Pipeline;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowSynchronousIOAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var bodyControlFeature = context.HttpContext.Features.Get<IHttpBodyControlFeature>();
        if (bodyControlFeature != null)
        {
            bodyControlFeature.AllowSynchronousIO = true;
        }
    }
}
