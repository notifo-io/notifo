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

            var endpoint = Simplify(Endpoint);

            if (user.WebPushSubscriptions.All(x => Simplify(x.Endpoint) != endpoint))
            {
                return default;
            }

            var newUser = user with
            {
                WebPushSubscriptions = user.WebPushSubscriptions.RemoveAll(x => Simplify(x.Endpoint) == endpoint)
            };

            return new ValueTask<User?>(newUser);
        }

        private static string Simplify(string url)
        {
            return Uri.UnescapeDataString(url);
        }
    }
}
