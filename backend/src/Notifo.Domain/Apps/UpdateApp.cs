// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Notifo.Domain.Identity;
using Notifo.Infrastructure;
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

        public Task<bool> ExecuteAsync(App app, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            if (Name != null)
            {
                app.Name = Name;
            }

            if (ConfirmUrl != null)
            {
                app.ConfirmUrl = ConfirmUrl;
            }

            if (Languages != null)
            {
                app.Languages = Languages;
            }

            if (app.ApiKeys.Count == 0)
            {
                app.ApiKeys[RandomHash.New()] = NotifoRoles.AppAdmin;
                app.ApiKeys[RandomHash.New()] = NotifoRoles.AppAdmin;
                app.ApiKeys[RandomHash.New()] = NotifoRoles.AppWebManager;
                app.ApiKeys[RandomHash.New()] = NotifoRoles.AppWebManager;
            }

            if (app.Contributors.Count == 0 && !string.IsNullOrWhiteSpace(UserId))
            {
                app.Contributors[UserId] = NotifoRoles.AppOwner;
            }

            return Task.FromResult(true);
        }
    }
}
