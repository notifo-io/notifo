// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Notifo.Domain.Identity;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Apps
{
    public sealed class UpdateApp : ICommand<App>
    {
        public string UserId { get; set; }

        public string? Name { get; set; }

        public string? ConfirmUrl { get; set; }

        public string[]? Languages { get; set; }

        public bool CanCreate => true;

        private sealed class Validator : AbstractValidator<UpdateApp>
        {
            public Validator()
            {
                RuleForEach(x => x.Languages).Language();
                RuleFor(x => x.ConfirmUrl).Url();
            }
        }

        public ValueTask<App?> ExecuteAsync(App app, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var newApp = app;

            if (Languages?.Length > 0 && !Languages.EqualsList(app.Languages))
            {
                newApp = newApp with
                {
                    Languages = Languages.ToImmutableList()
                };
            }

            if (Name != null && !string.Equals(Name, app.Name, StringComparison.Ordinal))
            {
                newApp = newApp with
                {
                    Name = Name.Trim()
                };
            }

            if (ConfirmUrl != null && !string.Equals(ConfirmUrl, app.ConfirmUrl, StringComparison.Ordinal))
            {
                newApp = newApp with
                {
                    ConfirmUrl = ConfirmUrl.Trim()
                };
            }

            if (app.ApiKeys.Count == 0)
            {
                newApp = newApp with
                {
                    ApiKeys = new Dictionary<string, string>
                    {
                        [RandomHash.New()] = NotifoRoles.AppAdmin,
                        [RandomHash.New()] = NotifoRoles.AppAdmin,
                        [RandomHash.New()] = NotifoRoles.AppWebManager,
                        [RandomHash.New()] = NotifoRoles.AppWebManager
                    }.ToImmutableDictionary()
                };
            }

            if (app.Contributors.Count == 0 && !string.IsNullOrWhiteSpace(UserId))
            {
                newApp = newApp with
                {
                    Contributors = new Dictionary<string, string>
                    {
                        [UserId] = NotifoRoles.AppOwner
                    }.ToImmutableDictionary()
                };
            }

            return new ValueTask<App?>(newApp);
        }
    }
}
