// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Identity;
using Notifo.Identity;

namespace Notifo.Areas.Account.Pages
{
    public abstract class PageModelBase<TDerived> : PageModel
    {
        private readonly Lazy<SignInManager<IdentityUser>> signInManager;
        private readonly Lazy<IUserService> userService;
        private readonly Lazy<ILogger<TDerived>> logger;
        private readonly Lazy<IStringLocalizer<AppResources>> localizer;

        public SignInManager<IdentityUser> SignInManager
        {
            get => signInManager.Value;
        }

        public IUserService UserService
        {
            get => userService.Value;
        }

        public ILogger<TDerived> Logger
        {
            get => logger.Value;
        }

        public IStringLocalizer<AppResources> T
        {
            get => localizer.Value;
        }

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        protected PageModelBase()
        {
            SetupService(ref logger!);
            SetupService(ref localizer!);
            SetupService(ref signInManager!);
            SetupService(ref userService!);
        }

        private void SetupService<TService>(ref Lazy<TService>? value) where TService : notnull
        {
            value = new Lazy<TService>(() => HttpContext.RequestServices.GetRequiredService<TService>());
        }

        protected Task<IActionResult> RedirectToErrorPage(string error, string errorDescription)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                errorDescription = $"{error}: {errorDescription}";
            }

            throw new InvalidOperationException(errorDescription);
        }

        protected IActionResult RedirectTo(string? returnUrl)
        {
            if (Uri.IsWellFormedUriString(returnUrl, UriKind.RelativeOrAbsolute))
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return Redirect("~/");
            }
        }

        protected async Task<IUser> GetUserAsync()
        {
            var user = await UserService.GetAsync(User);

            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{UserService.GetUserId(User)}'.");
            }

            return user;
        }
    }
}
