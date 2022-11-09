// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Domain.Apps;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Security;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers;

[ApiController]
[ApiExceptionFilter]
[ApiModelValidation(true)]
public abstract class BaseController : Controller
{
    protected string UserId
    {
        get
        {
            var id = User.UserId();

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new DomainForbiddenException("This operation is only allowed using User API Keys.");
            }

            return id;
        }
    }

    protected string UserIdOrSub => User.UserId() ?? User.Sub()!;

    public App App
    {
        get => HttpContext.Features.Get<IAppFeature>()!.App;
    }
}
