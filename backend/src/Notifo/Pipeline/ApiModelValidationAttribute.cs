// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Notifo.Infrastructure.Validation;

namespace Notifo.Pipeline;

public sealed class ApiModelValidationAttribute : ActionFilterAttribute
{
    private readonly bool allErrors;

    public ApiModelValidationAttribute(bool allErrors)
    {
        this.allErrors = allErrors;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = new List<ValidationError>();

            foreach (var (key, value) in context.ModelState)
            {
                if (value.ValidationState == ModelValidationState.Invalid)
                {
                    var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<AppResources>>();

                    if (string.IsNullOrWhiteSpace(key))
                    {
                        errors.Add(new ValidationError(localizer["InvalidRequestFormat"].ToString()));
                    }
                    else
                    {
                        foreach (var error in value.Errors)
                        {
                            if (!string.IsNullOrWhiteSpace(error.ErrorMessage) && allErrors)
                            {
                                errors.Add(new ValidationError(error.ErrorMessage));
                            }
                            else if (error.Exception is JsonException jsonException)
                            {
                                errors.Add(new ValidationError(jsonException.Message));
                            }
                        }
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new ValidationException(errors);
            }
        }
    }
}
