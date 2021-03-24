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
    public sealed class AddUserWebPushSubscription : ICommand<User>
    {
        public WebPushSubscription Subscription { get; set; }

        private sealed class Validator : AbstractValidator<AddUserWebPushSubscription>
        {
            public Validator()
            {
                RuleFor(x => x.Subscription).NotNull();
                RuleFor(x => x.Subscription.Endpoint).NotNull().NotEmpty();
            }
        }

        public Task ExecuteAsync(User user, IServiceProvider serviceProvider, CancellationToken ct)
        {
            Validate<Validator>.It(this);

            user.WebPushSubscriptions.Add(Subscription);

            return Task.CompletedTask;
        }
    }
}
