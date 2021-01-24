// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
        private readonly Lazy<IEventService> events;
        private readonly Lazy<IStringLocalizer<AppResources>> localizer;

        public SignInManager<IdentityUser> SignInManager
        {
            get { return signInManager.Value; }
        }

        public IUserService UserService
        {
            get { return userService.Value; }
        }

        public ILogger<TDerived> Logger
        {
            get { return logger.Value; }
        }

        public IStringLocalizer<AppResources> T
        {
            get { return localizer.Value; }
        }

        public IEventService Events
        {
            get { return events.Value; }
        }

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        protected PageModelBase()
        {
            SetupService(ref events!);
            SetupService(ref logger!);
            SetupService(ref localizer!);
            SetupService(ref signInManager!);
            SetupService(ref userService!);
        }

        private void SetupService<TService>(ref Lazy<TService>? value) where TService : notnull
        {
            value = new Lazy<TService>(() => HttpContext.RequestServices.GetRequiredService<TService>());
        }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);
        }

        protected async Task<IActionResult> RedirectToErrorPage(string error, string errorDescription)
        {
            var errorModel = new ErrorMessage
            {
                RequestId = HttpContext.TraceIdentifier,
                Error = error,
                ErrorDescription = errorDescription,
            };

            var store = HttpContext.RequestServices.GetRequiredService<IMessageStore<ErrorMessage>>();

            var errorMessage = new Message<ErrorMessage>(errorModel, DateTime.UtcNow);
            var errorId = await store.WriteAsync(errorMessage);

            return RedirectToPage("Error", new { errorId });
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
