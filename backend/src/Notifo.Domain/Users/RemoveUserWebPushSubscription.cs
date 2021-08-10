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
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Users
{
    public sealed class RemoveUserWebPushSubscription : ICommand<User>
    {
        public string Endpoint { get; set; }

        private sealed class Validator : AbstractValidator<RemoveUserWebPushSubscription>
        {
            public Validator()
            {
                RuleFor(x => x.Endpoint).NotNull().NotEmpty();
            }
        }

        public Task<bool> ExecuteAsync(User user, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var removed = user.WebPushSubscriptions.RemoveWhere(x => x.Endpoint == Endpoint);

            return Task.FromResult(removed > 0);
        }
    }
}
