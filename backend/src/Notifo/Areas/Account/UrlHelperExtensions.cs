// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Notifo.Areas.Account;

public static class UrlHelperExtensions
{
    public static string? EmailConfirmationLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
    {
        return urlHelper.Page("/ConfirmEmail", null, new { userId, code }, scheme);
    }

    public static string? ResetPasswordCallbackLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
    {
        return urlHelper.Page("/Account/ResetPassword", null, new { userId, code }, scheme);
    }

    public static void AddModelErrors(this ModelStateDictionary modelState, IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(string.Empty, error.Description);
        }
    }

    public static string? ActiveClass(this ViewContext viewContext, string page)
    {
        var activePage = viewContext.ViewData["ActivePage"] as string
            ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);

        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
    }
}
