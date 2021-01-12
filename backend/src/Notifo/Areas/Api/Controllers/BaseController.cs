// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;
using Notifo.Domain.Apps;
using Notifo.Infrastructure.Security;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers
{
    [ApiController]
    [ApiExceptionFilter]
    [ApiModelValidation(true)]
    public abstract class BaseController : Controller
    {
        protected string UserId
        {
            get
            {
                var id = HttpContext.User.UserId();

                if (id == null)
                {
                    throw new InvalidOperationException("Not in an authorized context.");
                }

                return id;
            }
        }

        protected App App
        {
            get
            {
                var app = HttpContext.Features.Get<IAppFeature>()?.App;

                if (app == null)
                {
                    throw new InvalidOperationException("Not in an app context.");
                }

                return app;
            }
        }
    }
}
