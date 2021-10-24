// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Users
{
    public sealed class RemoveUserMobileToken : ICommand<User>
    {
        public string Token { get; set; }

        private sealed class Validator : AbstractValidator<RemoveUserMobileToken>
        {
            public Validator()
            {
                RuleFor(x => x.Token).NotNull().NotEmpty();
            }
        }

        public ValueTask<User?> ExecuteAsync(User user, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            if (!user.MobilePushTokens.Any(x => x.Token != Token))
            {
                return default;
            }

            var newUser = user with
            {
                MobilePushTokens = user.MobilePushTokens.Where(x => x.Token != Token).ToImmutableList()
            };

            return new ValueTask<User?>(newUser);
        }
    }
}
