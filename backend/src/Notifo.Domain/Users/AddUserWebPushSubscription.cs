// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Domain.Channels.WebPush;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Users;

public sealed class AddUserWebPushSubscription : UserCommand
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

    public override ValueTask<User?> ExecuteAsync(User target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        if (target.WebPushSubscriptions.Any(x => x.Endpoint == Subscription.Endpoint))
        {
            return default;
        }

        var newWebPushSubscriptions = new List<WebPushSubscription>(target.WebPushSubscriptions)
        {
            Subscription
        };

        var newUser = target with
        {
            WebPushSubscriptions = newWebPushSubscriptions.ToReadonlyList()
        };

        return new ValueTask<User?>(newUser);
    }
}
