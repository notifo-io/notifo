// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Notifo.Areas.Account.Pages.Utils;
using Notifo.Identity;
using Notifo.Infrastructure.Tasks;
using NotifoValidationException = Notifo.Infrastructure.Validation.ValidationException;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Areas.Account.Pages;

[Authorize]
public sealed class ProfileModel : PageModelBase<ProfileModel>
{
    public bool HasPassword { get; set; }

    public IList<UserLoginInfo> ExternalLogins { get; set; }

    public IList<AuthenticationScheme> ExternalProviders { get; set; }

    public RemoveLoginModel RemoveLoginForm { get; set; } = new RemoveLoginModel();

    public ChangeProfileModel ChangeForm { get; set; } = new ChangeProfileModel();

    public ChangePasswordModel ChangePasswordForm { get; set; } = new ChangePasswordModel();

    public SetPasswordModel SetPasswordForm { get; set; } = new SetPasswordModel();

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var user = await UserService.GetAsync(User, HttpContext.RequestAborted);

        if (user == null)
        {
            Response.StatusCode = 404;
            return;
        }

        var (providers, hasPassword, logins) = await AsyncHelper.WhenAll(
            SignInManager.GetExternalAuthenticationSchemesAsync(),
            UserService.HasPasswordAsync(user, HttpContext.RequestAborted),
            UserService.GetLoginsAsync(user, HttpContext.RequestAborted));

        ChangeForm.Email ??= user.Email;
        ExternalLogins = logins;
        ExternalProviders = providers.ToList();
        HasPassword = hasPassword;

        await next();
    }

    public void OnGet()
    {
    }

    public Task<IActionResult> OnGetLoginCallback()
    {
        return MakeChangeAsync((id, _) => AddLoginAsync(id),
            T["UsersProfileAddLoginDone"]);
    }

    public Task<IActionResult> OnPost(
        [FromForm] ChangeProfileModel form)
    {
        ChangeForm = form with { IsActive = true };

        return MakeChangeAsync((id, ct) => UserService.UpdateAsync(id, new UserValues { Email = form.Email }, ct: ct),
            T["UsersProfileUpdateProfileDone"]);
    }

    public Task<IActionResult> OnPostRemoveLogin(
        [FromForm] RemoveLoginModel form)
    {
        RemoveLoginForm = form with { IsActive = true };

        return MakeChangeAsync((id, ct) => UserService.RemoveLoginAsync(id, form.LoginProvider, form.ProviderKey, ct),
            T["UsersProfileRemoveLoginDone"]);
    }

    public Task<IActionResult> OnPostSetPassword(
        [FromForm] SetPasswordModel form)
    {
        SetPasswordForm = form with { IsActive = true };

        return MakeChangeAsync((id, ct) => UserService.SetPasswordAsync(id, form.Password, ct: ct),
            T["UsersProfileSetPasswordDone"]);
    }

    public Task<IActionResult> OnPostChangePassword(
        [FromForm] ChangePasswordModel form)
    {
        ChangePasswordForm = form with { IsActive = true };

        return MakeChangeAsync((id, ct) => UserService.SetPasswordAsync(id, form.Password, form.OldPassword, ct),
            T["UsersProfileChangePasswordDone"]);
    }

    public async Task<IActionResult> OnPostAddLogin(string provider)
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        var userId = UserService.GetUserId(User, HttpContext.RequestAborted);

        var challengeRedirectUrl = Url.Page(null, "LoginCallback", new { ReturnUrl });
        var challengeProperties = SignInManager.ConfigureExternalAuthenticationProperties(provider, challengeRedirectUrl, userId);

        return Challenge(challengeProperties, provider);
    }

    private async Task AddLoginAsync(string id)
    {
        var loginInfo = await SignInManager.GetExternalLoginInfoAsync(id)
            ?? throw new ValidationException(T["ExternalLoginError"]);

        var email = loginInfo.Principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException(T["GithubEmailPrivateError"]);
        }

        loginInfo.ProviderDisplayName = email;

        await UserService.AddLoginAsync(id, loginInfo, HttpContext.RequestAborted);
    }

    private async Task<IActionResult> MakeChangeAsync(Func<string, CancellationToken, Task> action, string statusMessage)
    {
        var user = (await UserService.GetAsync(User, HttpContext.RequestAborted))!;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await action(user.Id, HttpContext.RequestAborted);

            await SignInManager.SignInAsync((IdentityUser)user.Identity, true);

            return RedirectToPage(new { statusMessage });
        }
        catch (NotifoValidationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (ValidationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            ErrorMessage = T["GeneralError"];
        }

        return Page();
    }
}

public sealed record ChangePasswordModel
{
    [Required]
    [Display(Name = nameof(OldPassword))]
    public string OldPassword { get; set; }

    [Required]
    [Display(Name = nameof(Password))]
    public string Password { get; set; }

    [Compare(nameof(Password))]
    [Display(Name = nameof(PasswordConfirm))]
    public string PasswordConfirm { get; set; }

    public bool IsActive { get; set; }
}

public sealed record SetPasswordModel
{
    [Required]
    [Display(Name = nameof(Password))]
    public string Password { get; set; }

    [Required]
    [Display(Name = nameof(PasswordConfirm))]
    public string PasswordConfirm { get; set; }

    public bool IsActive { get; set; }
}

public sealed record RemoveLoginModel
{
    [Required]
    [Display(Name = nameof(LoginProvider))]
    public string LoginProvider { get; set; }

    [Required]
    [Display(Name = nameof(ProviderKey))]
    public string ProviderKey { get; set; }

    public bool IsActive { get; set; }
}

public sealed record ChangeProfileModel
{
    [Required]
    [EmailAddress]
    [Display(Name = nameof(Email))]
    public string Email { get; set; }

    public bool IsActive { get; set; }
}
