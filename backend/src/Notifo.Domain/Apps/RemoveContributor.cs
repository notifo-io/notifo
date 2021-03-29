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
using Notifo.Domain.Resources;
using Notifo.Infrastructure;
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

        public Task<bool> ExecuteAsync(App app, IServiceProvider serviceProvider, CancellationToken ct)
        {
            Validate<Validator>.It(this);

            if (string.Equals(ContributorId, UserId, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainException(Texts.App_CannotRemoveYourself);
            }

            var removed = app.Contributors.Remove(ContributorId);

            return Task.FromResult(removed);
        }
    }
}
