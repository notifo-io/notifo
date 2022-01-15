// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Notifo.Areas.Account.Pages.Utils;
using Notifo.Identity;
using Notifo.Infrastructure.Reflection;
using Squidex.Assets;

namespace Notifo.Areas.Account.Pages
{
    public sealed class SetupModel : PageModelBase<SetupModel>
    {
        private readonly IAssetStore assetStore;

        public string Email { get; set; }

        public string BaseUrlCurrent { get; set; }

        public string BaseUrlConfigured { get; set; }

        public bool IsValidHttps { get; set; }

        public bool IsAssetStoreFtp { get; set; }

        public bool IsAssetStoreFile { get; set; }

        public bool HasExternalLogin { get; set; }

        public bool HasPasswordAuth { get; set; } = true;

        public SetupModel(IAssetStore assetStore)
        {
            this.assetStore = assetStore;
        }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            await next();

            var externalProviders = await SignInManager.GetExternalAuthenticationSchemesAsync();

            BaseUrlConfigured = UrlGenerator.BuildUrl(string.Empty, false);
            BaseUrlCurrent = GetCurrentUrl();
            IsValidHttps = HttpContext.Request.IsHttps;
            IsAssetStoreFile = assetStore is FolderAssetStore;
            IsAssetStoreFtp = assetStore is FTPAssetStore;
            HasExternalLogin = externalProviders.Any();
        }

        public async Task<IActionResult> OnGet()
        {
            if (!await UserService.IsEmptyAsync(HttpContext.RequestAborted))
            {
                return RedirectTo("~/");
            }

            return Page();
        }

        public async Task<IActionResult> OnPost(CreateUserModel model)
        {
            if (!await UserService.IsEmptyAsync(HttpContext.RequestAborted))
            {
                return RedirectTo("~/");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string errorMessage;
            try
            {
                var user = await UserService.CreateAsync(model.Email, new UserValues
                {
                    Password = model.Password
                }, ct: HttpContext.RequestAborted);

                await SignInManager.SignInAsync((IdentityUser)user.Identity, true);

                return RedirectTo("~/");
            }
            catch (ValidationException ex)
            {
                errorMessage = ex.Message;
            }
            catch (Exception)
            {
                ErrorMessage = T["SetupError"];
            }

            SimpleMapper.Map(model, this);

            return Page();
        }

        private string GetCurrentUrl()
        {
            var request = HttpContext.Request;

            var url = $"{request.Scheme}://{request.Host}{request.PathBase}";

            return url.TrimEnd('/');
        }
    }

    public sealed class CreateUserModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string PasswordConfirm { get; set; }
    }
}
