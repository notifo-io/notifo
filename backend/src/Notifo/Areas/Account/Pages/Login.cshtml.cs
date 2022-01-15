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

namespace Notifo.Areas.Account.Pages
{
    public sealed class LoginModel : PageModelBase<LoginModel>
    {
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public bool HasPasswordAuth { get; set; } = true;

        [BindProperty]
        public LoginInputModel Input { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            ExternalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            await next();
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var result = await SignInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, true);

                if (result.Succeeded)
                {
                    return RedirectTo(ReturnUrl);
                }

                if (result.IsLockedOut)
                {
                    return RedirectToPage("./Lockout");
                }

                ModelState.AddModelError(string.Empty, T["InvalidLoginAttempt"]!);
            }

            return Page();
        }
    }

    public sealed class LoginInputModel
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
    }
}
