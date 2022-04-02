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
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Apps
{
    public sealed class UpsertApp : ICommand<App>
    {
        public string UserId { get; set; }

        public string? Name { get; set; }

        public string? ConfirmUrl { get; set; }

        public string[]? Languages { get; set; }

        public bool CanCreate => true;

        private sealed class Validator : AbstractValidator<UpsertApp>
        {
            public Validator()
            {
                RuleForEach(x => x.Languages).Language();
                RuleFor(x => x.ConfirmUrl).Url();
            }
        }

        public async ValueTask<App?> ExecuteAsync(App app, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var newApp = app;

            if (Is.IsChanged(Languages, app.Languages))
            {
                newApp = newApp with
                {
                    Languages = Languages.ToReadonlyList()
                };
            }

            if (Is.IsChanged(Name, app.Name))
            {
                newApp = newApp with
                {
                    Name = Name.Trim()
                };
            }

            if (Is.IsChanged(ConfirmUrl, app.ConfirmUrl))
            {
                newApp = newApp with
                {
                    ConfirmUrl = ConfirmUrl.Trim()
                };
            }

            if (app.ApiKeys.Count == 0)
            {
                var apiKeyGenerator = serviceProvider.GetRequiredService<IApiKeyGenerator>();

                newApp = newApp with
                {
                    ApiKeys = new Dictionary<string, string>
                    {
                        [await apiKeyGenerator.GenerateAppTokenAsync(app.Id)] = NotifoRoles.AppAdmin,
                        [await apiKeyGenerator.GenerateAppTokenAsync(app.Id)] = NotifoRoles.AppAdmin,
                        [await apiKeyGenerator.GenerateAppTokenAsync(app.Id)] = NotifoRoles.AppWebManager,
                        [await apiKeyGenerator.GenerateAppTokenAsync(app.Id)] = NotifoRoles.AppWebManager
                    }.ToReadonlyDictionary()
                };
            }

            if (app.Contributors.Count == 0 && !string.IsNullOrWhiteSpace(UserId))
            {
                newApp = newApp with
                {
                    Contributors = new Dictionary<string, string>
                    {
                        [UserId] = NotifoRoles.AppOwner
                    }.ToReadonlyDictionary()
                };
            }

            return newApp;
        }
    }
}
