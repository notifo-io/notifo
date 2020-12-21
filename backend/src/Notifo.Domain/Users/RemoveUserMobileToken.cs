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

        public Task ExecuteAsync(User user, IServiceProvider serviceProvider, CancellationToken ct)
        {
            Validate<Validator>.It(this);

            user.MobilePushTokens.Remove(Token);

            return Task.CompletedTask;
        }
    }
}
