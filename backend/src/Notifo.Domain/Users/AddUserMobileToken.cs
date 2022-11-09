// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Users;

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

    public ValueTask<User?> ExecuteAsync(User user, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        if (user.MobilePushTokens.Any(x => x.Token == Token.Token))
        {
            return default;
        }

        var newMobilePushTokens = new List<MobilePushToken>(user.MobilePushTokens)
        {
            Token
        };

        var newUser = user with
        {
            MobilePushTokens = newMobilePushTokens.ToReadonlyList()
        };

        return new ValueTask<User?>(newUser);
    }
}
