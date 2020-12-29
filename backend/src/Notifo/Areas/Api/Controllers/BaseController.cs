// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
        protected string UserId => User.UserId() ?? User.OpenIdSubject()!;

        public App App
        {
            get { return HttpContext.Features.Get<IAppFeature>().App; }
        }
    }
}
