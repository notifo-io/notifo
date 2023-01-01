// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Log;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Users;

public sealed class RemoveUserWebPushSubscription : UserCommand
{
    public string Endpoint { get; set; }

    private sealed class Validator : AbstractValidator<RemoveUserWebPushSubscription>
    {
        public Validator()
        {
            RuleFor(x => x.Endpoint).NotNull().NotEmpty();
        }
    }

    public override ValueTask<User?> ExecuteAsync(User target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        var endpoint = Simplify(Endpoint);

        if (target.WebPushSubscriptions.All(x => Simplify(x.Endpoint) != endpoint))
        {
            return default;
        }

        var newUser = target with
        {
            WebPushSubscriptions = target.WebPushSubscriptions.RemoveAll(x => Simplify(x.Endpoint) == endpoint)
        };

        return new ValueTask<User?>(newUser);
    }

    public override ValueTask ExecutedAsync(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<ILogStore>()
            .LogAsync(AppId, UserId, LogMessage.WebPush_TokenRemoved("System", UserId, Endpoint));

        return default;
    }

    private static string Simplify(string url)
    {
        return Uri.UnescapeDataString(url);
    }
}
