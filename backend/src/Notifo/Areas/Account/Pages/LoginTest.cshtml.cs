// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Account.Pages.Utils;
using Notifo.Domain.Apps;
using Notifo.Identity.Dynamic;
using Notifo.Infrastructure;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Areas.Account.Pages;

public sealed class LoginTestModel(DynamicSchemeProvider schemes) : PageModelBase<ExternalLoginModel>
{
    [BindProperty]
    public ConfirmationForm Model { get; set; } = new ConfirmationForm();

    public async Task<IActionResult> OnGet(
        [FromQuery] AppAuthScheme scheme)
    {
        var id = await schemes.AddTemporarySchemeAsync(scheme, default);

        var challengeRedirectUrl = Url.Page(null, "Callback");
        var challengeProperties = SignInManager.ConfigureExternalAuthenticationProperties(id, challengeRedirectUrl);

        return Challenge(challengeProperties, id);
    }

    public async Task<IActionResult> OnGetCallback(string? remoteError = null)
    {
        // This should actually never happen.
        if (remoteError != null)
        {
            ThrowHelper.InvalidOperationException(T["ExternalLoginError"]);
            return default!;
        }

        var loginInfo = await SignInManager.GetExternalLoginInfoAsync();

        // This should actually never happen.
        if (loginInfo == null)
        {
            return RedirectToPage("./Login");
        }

        var result = await SignInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, false);

        // Only redirect the user if he is not locked out manually or due too many invalid login attempts.
        if (result.Succeeded)
        {
            return RedirectTo(ReturnUrl);
        }

        return Page();
    }
}
