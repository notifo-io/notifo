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
using Notifo.Domain.Channels.MobilePush;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Users
{
    public sealed class AddUserMobileToken : ICommand<User>
    {
        public MobilePushToken Token { get; set; }

        private sealed class Validator : AbstractValidator<AddUserMobileToken>
        {
            public Validator()
            {
                RuleFor(x => x.Token).NotNull();
                RuleFor(x => x.Token.Token).NotNull().NotEmpty();
            }
        }

        public Task<bool> ExecuteAsync(User user, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            if (!user.MobilePushTokens.Contains(Token))
            {
                user.MobilePushTokens.Add(Token);

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
