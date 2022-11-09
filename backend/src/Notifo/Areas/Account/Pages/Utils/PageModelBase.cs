// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Notifo.Domain.Identity;
using Notifo.Identity;
using Squidex.Hosting;

namespace Notifo.Areas.Account.Pages.Utils;

public abstract class PageModelBase<TDerived> : PageModel
{
    private readonly Lazy<SignInManager<IdentityUser>> signInManager;
    private readonly Lazy<IUserService> userService;
    private readonly Lazy<IUrlGenerator> urlGenerator;
    private readonly Lazy<ILogger<TDerived>> logger;
    private readonly Lazy<IStringLocalizer<AppResources>> localizer;
    private readonly Lazy<IOptions<NotifoIdentityOptions>> options;

    public SignInManager<IdentityUser> SignInManager
    {
        get => signInManager.Value;
    }

    public IUserService UserService
    {
        get => userService.Value;
    }

    public IUrlGenerator UrlGenerator
    {
        get => urlGenerator.Value;
    }

    public ILogger<TDerived> Logger
    {
        get => logger.Value;
    }

    public IStringLocalizer<AppResources> T
    {
        get => localizer.Value;
    }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ErrorMessage { get; set; }

    public bool HasPasswordAuth => options.Value.Value.AllowPasswordAuth;

    protected PageModelBase()
    {
        SetupService(ref logger!);
        SetupService(ref localizer!);
        SetupService(ref signInManager!);
        SetupService(ref userService!);
        SetupService(ref urlGenerator!);
        SetupService(ref options!);
    }

    private void SetupService<TService>(ref Lazy<TService>? value) where TService : notnull
    {
        value = new Lazy<TService>(() => HttpContext.RequestServices.GetRequiredService<TService>());
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
        var user = await UserService.GetAsync(User, HttpContext.RequestAborted);

        if (user == null)
        {
            var userId = UserService.GetUserId(User, HttpContext.RequestAborted);

#pragma warning disable MA0014 // Do not raise System.ApplicationException type
            throw new ApplicationException($"Unable to load user with ID '{userId}'.");
#pragma warning restore MA0014 // Do not raise System.ApplicationException type
        }

        return user;
    }
}
