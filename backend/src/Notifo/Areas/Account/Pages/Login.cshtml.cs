// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Notifo.Areas.Account.Pages.Utils;
using Notifo.Identity.Dynamic;

#pragma warning disable MA0048 // File name must match type name

namespace Notifo.Areas.Account.Pages;

public sealed class LoginModel : PageModelBase<LoginModel>
{
    private readonly DynamicSchemeProvider schemeProvider;

    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public LoginEmailForm LoginEmailForm { get; set; } = new LoginEmailForm();

    public LoginDynamicForm LoginDynamicForm { get; set; } = new LoginDynamicForm();

    public bool HasDynamicAuthScheme { get; set; }

    public LoginEmailForm LoginEmailForm { get; set; } = new LoginEmailForm();

    public LoginDynamicForm LoginDynamicForm { get; set; } = new LoginDynamicForm();

    [BindProperty(SupportsGet = true)]
    public bool Signup { get; set; }

    public LoginModel(DynamicSchemeProvider schemeProvider)
    {
        this.schemeProvider = schemeProvider;
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        HasDynamicAuthScheme = await schemeProvider.HasCustomSchemeAsync();

        ExternalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        await next();
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(
        [FromForm(Name = "LoginEmailForm")] LoginEmailForm form)
    {
        LoginEmailForm = form with { IsActive = true };

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await SignInManager.PasswordSignInAsync(form.Email, form.Password, form.RememberMe, true);

        if (result.Succeeded)
        {
            return RedirectTo(ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, T["InvalidLoginAttempt"]!);

        return Page();
    }

    public async Task<IActionResult> OnPostDynamic(
        [FromForm(Name = "LoginDynamicForm")] LoginDynamicForm form)
    {
        LoginDynamicForm = form with { IsActive = true };

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var scheme = await schemeProvider.GetSchemaByEmailAddressAsync(form.Email);

        if (scheme != null)
        {
            var provider = scheme.Name;

            var challengeRedirectUrl = Url.Page("ExternalLogin", "Callback", new { ReturnUrl });
            var challengeProperties = SignInManager.ConfigureExternalAuthenticationProperties(provider, challengeRedirectUrl);

            return Challenge(challengeProperties, provider);
        }

        ModelState.AddModelError(string.Empty, T["LoginCustomNoProvider"]!);

        return Page();
    }
}

public sealed record LoginEmailForm
{
    [Required]
    [EmailAddress]
    [Display(Name = nameof(Email))]
    public string Email { get; set; }

    [Required]
    [Display(Name = nameof(Password))]
    public string Password { get; set; }

    [Required]
    [Display(Name = nameof(RememberMe))]
    public bool RememberMe { get; set; }

    public bool IsActive { get; set; }
}

public sealed record LoginDynamicForm
{
    [Required]
    [EmailAddress]
    [Display(Name = nameof(Email))]
    public string Email { get; set; }

    public bool IsActive { get; set; }
}
