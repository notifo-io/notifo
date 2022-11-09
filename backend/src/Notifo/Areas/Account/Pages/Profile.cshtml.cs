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

    [BindProperty]
    public ChangeProfileModel Model { get; set; } = new ChangeProfileModel();

    [BindProperty]
    public ChangePasswordModel ChangePasswordModel { get; set; } = new ChangePasswordModel();

    [BindProperty]
    public SetPasswordModel SetPasswordModel { get; set; } = new SetPasswordModel();

    [BindProperty]
    public RemoveLoginModel RemoveLoginModel { get; set; } = new RemoveLoginModel();

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

        Model.Email ??= user.Email;
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

    public Task<IActionResult> OnPost()
    {
        ClearErrors(nameof(Model));

        return MakeChangeAsync((id, ct) => UserService.UpdateAsync(id, new UserValues { Email = Model.Email }, ct: ct),
            T["UsersProfileUpdateProfileDone"]);
    }

    public Task<IActionResult> OnPostRemoveLogin()
    {
        ClearErrors(nameof(RemoveLoginModel));

        return MakeChangeAsync((id, ct) => UserService.RemoveLoginAsync(id, RemoveLoginModel.LoginProvider, RemoveLoginModel.ProviderKey, ct),
            T["UsersProfileRemoveLoginDone"]);
    }

    public Task<IActionResult> OnPostSetPassword()
    {
        ClearErrors(nameof(SetPasswordModel));

        return MakeChangeAsync((id, ct) => UserService.SetPasswordAsync(id, SetPasswordModel.Password, ct: ct),
            T["UsersProfileSetPasswordDone"]);
    }

    public Task<IActionResult> OnPostChangePassword()
    {
        ClearErrors(nameof(ChangePasswordModel));

        return MakeChangeAsync((id, ct) => UserService.SetPasswordAsync(id, ChangePasswordModel.Password, ChangePasswordModel.OldPassword, ct),
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
        var loginInfo = await SignInManager.GetExternalLoginInfoAsync(id);

        if (loginInfo == null)
        {
            throw new ValidationException(T["ExternalLoginError"]);
        }

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

    private void ClearErrors(string prefix)
    {
        foreach (var errorKey in ModelState.Keys.ToList())
        {
            if (!errorKey.StartsWith(prefix, StringComparison.Ordinal))
            {
                ModelState.Remove(errorKey);
            }
        }
    }
}

public class ChangePasswordModel
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
}

public class SetPasswordModel
{
    [Required]
    [Display(Name = nameof(Password))]
    public string Password { get; set; }

    [Required]
    [Display(Name = nameof(PasswordConfirm))]
    public string PasswordConfirm { get; set; }
}

public class RemoveLoginModel
{
    [Required]
    [Display(Name = nameof(LoginProvider))]
    public string LoginProvider { get; set; }

    [Required]
    [Display(Name = nameof(ProviderKey))]
    public string ProviderKey { get; set; }
}

public class ChangeProfileModel
{
    [Required]
    [EmailAddress]
    [Display(Name = nameof(Email))]
    public string Email { get; set; }
}
