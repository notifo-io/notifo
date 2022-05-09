// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
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

        public ValueTask<User?> ExecuteAsync(User user, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            if (!user.WebPushSubscriptions.Any(x => x.Endpoint == Endpoint))
            {
                return default;
            }

            var newUser = user with
            {
                WebPushSubscriptions = user.WebPushSubscriptions.Where(x => x.Endpoint != Endpoint).ToReadonlyList()
            };

            return new ValueTask<User?>(newUser);
        }
    }
}
