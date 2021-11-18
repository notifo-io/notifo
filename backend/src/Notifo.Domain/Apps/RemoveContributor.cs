// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Domain.Resources;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Apps
{
    public sealed class RemoveContributor : ICommand<App>
    {
        public string ContributorId { get; set; }

        public string UserId { get; set; }

        private sealed class Validator : AbstractValidator<RemoveContributor>
        {
            public Validator()
            {
                RuleFor(x => x.ContributorId).NotNull().NotEmpty();
                RuleFor(x => x.UserId).NotNull().NotEmpty();
            }
        }

        public ValueTask<App?> ExecuteAsync(App app, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            if (string.Equals(ContributorId, UserId, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainException(Texts.App_CannotRemoveYourself);
            }

            if (!app.Contributors.ContainsKey(ContributorId))
            {
                return default;
            }

            var newApp = app with
            {
                Contributors = app.Contributors.Where(x => x.Key != ContributorId).ToReadonlyDictionary()
            };

            return new ValueTask<App?>(newApp);
        }
    }
}
