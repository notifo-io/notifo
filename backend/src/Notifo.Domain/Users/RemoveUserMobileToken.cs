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

namespace Notifo.Domain.Users;

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

        var token = Simplify(Token);

        if (user.MobilePushTokens.All(x => Simplify(x.Token) != token))
        {
            return default;
        }

        var newUser = user with
        {
            MobilePushTokens = user.MobilePushTokens.RemoveAll(x => Simplify(x.Token) == token)
        };

        return new ValueTask<User?>(newUser);
    }

    private static string Simplify(string url)
    {
        return Uri.UnescapeDataString(url);
    }
}
