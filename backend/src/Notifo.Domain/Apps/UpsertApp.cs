// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Identity;
using Notifo.Domain.Utils;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Apps;

public sealed class UpsertApp : AppCommand
{
    public string? Name { get; set; }

    public string? ConfirmUrl { get; set; }

    public string[]? Languages { get; set; }

    public override bool CanCreate => true;

    private sealed class Validator : AbstractValidator<UpsertApp>
    {
        public Validator()
        {
            RuleForEach(x => x.Languages).Language();
            RuleFor(x => x.ConfirmUrl).Url();
        }
    }

    public override async ValueTask<App?> ExecuteAsync(App target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        var newApp = target;

        if (Is.Changed(Languages, target.Languages))
        {
            newApp = newApp with
            {
                Languages = Languages.ToReadonlyList()
            };
        }

        if (Is.Changed(Name, target.Name))
        {
            newApp = newApp with
            {
                Name = Name.Trim()
            };
        }

        if (Is.Changed(ConfirmUrl, target.ConfirmUrl))
        {
            newApp = newApp with
            {
                ConfirmUrl = ConfirmUrl.Trim()
            };
        }

        if (target.ApiKeys.Count == 0)
        {
            var apiKeyGenerator = serviceProvider.GetRequiredService<IApiKeyGenerator>();

            newApp = newApp with
            {
                ApiKeys = new Dictionary<string, string>
                {
                    [await apiKeyGenerator.GenerateAppTokenAsync(target.Id)] = NotifoRoles.AppAdmin,
                    [await apiKeyGenerator.GenerateAppTokenAsync(target.Id)] = NotifoRoles.AppAdmin,
                    [await apiKeyGenerator.GenerateAppTokenAsync(target.Id)] = NotifoRoles.AppWebManager,
                    [await apiKeyGenerator.GenerateAppTokenAsync(target.Id)] = NotifoRoles.AppWebManager
                }.ToReadonlyDictionary()
            };
        }

        if (target.Contributors.Count == 0 && !string.IsNullOrWhiteSpace(PrincipalId))
        {
            newApp = newApp with
            {
                Contributors = new Dictionary<string, string>
                {
                    [PrincipalId] = NotifoRoles.AppOwner
                }.ToReadonlyDictionary()
            };
        }

        return newApp;
    }
}
