// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Areas.Account.Pages;

public sealed class ErrorModel : PageModel
{
    public string? ErrorMessage { get; set; }

    public string? ErrorCode { get; set; } = "400";

    public void OnGet()
    {
        var response = HttpContext.GetOpenIddictServerResponse();

        ErrorMessage = response?.ErrorDescription;
        ErrorCode = response?.Error ?? "400";

        if (string.IsNullOrWhiteSpace(ErrorMessage))
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

            while (exception?.InnerException != null)
            {
                exception = exception.InnerException;
            }

            ErrorMessage = exception?.Message;
        }
    }
}
