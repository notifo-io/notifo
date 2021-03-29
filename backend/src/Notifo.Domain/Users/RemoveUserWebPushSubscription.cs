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
using Notifo.Domain.Channels.WebPush;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Users
{
    public sealed class RemoveUserWebPushSubscription : ICommand<User>
    {
        public WebPushSubscription Subscription { get; set; }

        private sealed class Validator : AbstractValidator<RemoveUserWebPushSubscription>
        {
            public Validator()
            {
                RuleFor(x => x.Subscription).NotNull().NotEmpty();
            }
        }

        public Task<bool> ExecuteAsync(User user, IServiceProvider serviceProvider, CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var removed = user.WebPushSubscriptions.Remove(Subscription);

            return Task.FromResult(removed);
        }
    }
}
