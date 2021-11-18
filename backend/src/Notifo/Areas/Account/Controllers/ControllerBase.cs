// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Notifo.Identity;

namespace Notifo.Areas.Account.Controllers
{
    public class ControllerBase<TDerived> : Controller
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

        protected ControllerBase()
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

        protected static Task<IActionResult> RedirectToErrorPage(string error, string errorDescription)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                errorDescription = $"{error}: {errorDescription}";
            }

            throw new InvalidOperationException(errorDescription);
        }
    }
}
