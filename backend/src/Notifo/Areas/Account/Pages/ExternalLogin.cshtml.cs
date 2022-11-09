// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Account.Pages.Utils;
using Notifo.Domain.Identity;
using Notifo.Infrastructure;
using NotifoValidationException = Notifo.Infrastructure.Validation.ValidationException;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Areas.Account.Pages;

public sealed class ExternalLoginModel : PageModelBase<ExternalLoginModel>
{
    public string LoginProvider { get; set; }

    public string TermsOfServiceUrl { get; set; }

    public string PrivacyPolicyUrl { get; set; }

    public bool MustAcceptsTermsOfService { get; set; } = true;

    public bool MustAcceptsPrivacyPolicy { get; set; } = true;

    [BindProperty]
    public ConfirmationModel Model { get; set; } = new ConfirmationModel();

    public IActionResult OnGet()
    {
        return RedirectToPage("./Login");
    }

    public async Task<IActionResult> OnGetCallback(string? remoteError = null)
    {
        if (remoteError != null)
        {
            ThrowHelper.InvalidOperationException(T["ExternalLoginError"]);
            return default!;
        }

        var loginInfo = await SignInManager.GetExternalLoginInfoAsync();

        if (loginInfo == null)
        {
            return RedirectToPage("./Login");
        }

        var result = await SignInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, false);

        if (result.Succeeded)
        {
            return RedirectTo(ReturnUrl);
        }
        else if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        LoginProvider = loginInfo.LoginProvider;

        var email = loginInfo.Principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            var errorMessage = T["GithubEmailPrivateError"];

            return RedirectToPage("./Login", new { errorMessage });
        }

        loginInfo.ProviderDisplayName = email;

        var existingUser = await UserService.FindByEmailAsync(email, HttpContext.RequestAborted);

        if (existingUser != null && await HasLoginAsync(existingUser))
        {
            var errorMessage = T["ExternalLoginEmailUsed"];

            return RedirectToPage("./Login", new { errorMessage });
        }

        return Page();
    }

    public IActionResult OnPost(string provider)
    {
        var challengeRedirectUrl = Url.Page(null, "Callback", new { ReturnUrl });
        var challengeProperties = SignInManager.ConfigureExternalAuthenticationProperties(provider, challengeRedirectUrl);

        return Challenge(challengeProperties, provider);
    }

    public async Task<IActionResult> OnPostConfirmation()
    {
        if (MustAcceptsPrivacyPolicy && !Model.AcceptPrivacyPolicy)
        {
            var field = nameof(Model.AcceptPrivacyPolicy);

            ModelState.AddModelError($"{nameof(Model)}.{field}", T[$"{field}Error"]!);
        }

        if (MustAcceptsTermsOfService && !Model.AcceptTermsOfService)
        {
            var field = nameof(Model.AcceptTermsOfService);

            ModelState.AddModelError($"{nameof(Model)}.{field}", T[$"{field}Error"]!);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var loginInfo = await SignInManager.GetExternalLoginInfoAsync();

        if (loginInfo == null)
        {
            ThrowHelper.InvalidOperationException(T["ExternalLoginError"]);
            return default!;
        }

        var email = loginInfo.Principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            var errorMessage = T["GithubEmailPrivateError"];

            return RedirectToPage("./Login", new { errorMessage });
        }

        loginInfo.ProviderDisplayName = email;

        IUser user;
        try
        {
            user = await UserService.CreateAsync(email, ct: HttpContext.RequestAborted);

            await UserService.AddLoginAsync(user.Id, loginInfo, HttpContext.RequestAborted);
        }
        catch (NotifoValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Errors[0].Message);
            return Page();
        }

        await SignInManager.SignInAsync((IdentityUser)user.Identity, false);

        return RedirectTo(ReturnUrl);
    }

    private async Task<bool> HasLoginAsync(IUser user)
    {
        if (await UserService.HasPasswordAsync(user, HttpContext.RequestAborted))
        {
            return true;
        }

        var logins = await UserService.GetLoginsAsync(user, HttpContext.RequestAborted);

        return logins.Count > 0;
    }
}

public sealed class ConfirmationModel
{
    [Required]
    [Display(Name = nameof(AcceptPrivacyPolicy))]
    public bool AcceptPrivacyPolicy { get; set; }

    [Required]
    [Display(Name = nameof(AcceptTermsOfService))]
    public bool AcceptTermsOfService { get; set; }
}
