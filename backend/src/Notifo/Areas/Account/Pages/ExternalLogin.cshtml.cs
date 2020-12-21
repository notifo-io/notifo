// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifo.Identity;

#pragma warning disable SA1649 // File name should match first type name

namespace Notifo.Areas.Account.Pages
{
    public sealed class ExternalLoginModel : PageModelBase<ExternalLoginModel>
    {
        private readonly IUserFactory userFactory;

        public string LoginProvider { get; set; }

        public string TermsOfServiceUrl { get; set; }

        public string PrivacyPolicyUrl { get; set; }

        public bool MustAcceptsTermsOfService { get; set; } = true;

        public bool MustAcceptsPrivacyPolicy { get; set; } = true;

        [BindProperty]
        public ExternalLoginInputModel Input { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public ExternalLoginModel(IUserFactory userFactory)
        {
            this.userFactory = userFactory;
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider)
        {
            var authenticationRedirectUrl = Url.Page("./ExternalLogin", "Callback", new { returnUrl = ReturnUrl });
            var authenticationProperties = SignInManager.ConfigureExternalAuthenticationProperties(provider, authenticationRedirectUrl);

            return new ChallengeResult(provider, authenticationProperties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = T["ExternalLoginError", remoteError];

                return RedirectToPage("./Login");
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
                return await RedirectToErrorPage("NoEmaiL", T["GithubEmailPrivateError"]!);
            }

            Input = new ExternalLoginInputModel();

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync()
        {
            if (MustAcceptsPrivacyPolicy && !Input.AcceptPrivacyPolicy)
            {
                var field = nameof(Input.AcceptPrivacyPolicy);

                ModelState.AddModelError($"{nameof(Input)}.{field}", T[$"{field}Error"]!);
            }

            if (MustAcceptsTermsOfService && !Input.AcceptTermsOfService)
            {
                var field = nameof(Input.AcceptTermsOfService);

                ModelState.AddModelError($"{nameof(Input)}.{field}", T[$"{field}Error"]!);
            }

            if (ModelState.IsValid)
            {
                var loginInfo = await SignInManager.GetExternalLoginInfoAsync();

                if (loginInfo == null)
                {
                    return await RedirectToErrorPage("NoEmaiL", T["GithubEmailPrivateError"]!);
                }

                var email = loginInfo.Principal.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new InvalidOperationException(T["GithubEmailPrivateError"]);
                }

                var user = await UserManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = userFactory.CreateUser(email);

                    var createResult = await UserManager.CreateAsync(user);

                    if (!createResult.Succeeded)
                    {
                        ModelState.AddModelErrors(createResult);
                        return Page();
                    }
                }

                var addResult = await UserManager.AddLoginAsync(user, loginInfo);

                if (!addResult.Succeeded)
                {
                    ModelState.AddModelErrors(addResult);
                    return Page();
                }

                await SignInManager.SignInAsync(user, false);

                return RedirectTo(ReturnUrl);
            }

            return Page();
        }
    }

    public sealed class ExternalLoginInputModel
    {
        [Required]
        [Display(Name = nameof(AcceptPrivacyPolicy))]
        public bool AcceptPrivacyPolicy { get; set; }

        [Required]
        [Display(Name = nameof(AcceptTermsOfService))]
        public bool AcceptTermsOfService { get; set; }
    }
}
