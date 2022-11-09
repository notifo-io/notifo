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

    public ValueTask<User?> ExecuteAsync(User user, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        if (user.WebPushSubscriptions.Any(x => x.Endpoint == Subscription.Endpoint))
        {
            return default;
        }

        var newWebPushSubscriptions = new List<WebPushSubscription>(user.WebPushSubscriptions)
        {
            Subscription
        };

        var newUser = user with
        {
            WebPushSubscriptions = newWebPushSubscriptions.ToReadonlyList()
        };

        return new ValueTask<User?>(newUser);
    }
}
