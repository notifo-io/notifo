// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Domain.Log;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Users;

public sealed class AddUserMobileToken : UserCommand
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

    public override ValueTask<User?> ExecuteAsync(User target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        if (target.MobilePushTokens.Any(x => x.Token == Token.Token))
        {
            return default;
        }

        var newMobilePushTokens = new List<MobilePushToken>(target.MobilePushTokens)
        {
            Token
        };

        var newUser = target with
        {
            MobilePushTokens = newMobilePushTokens.ToReadonlyList()
        };

        return new ValueTask<User?>(newUser);
    }

    public override ValueTask ExecutedAsync(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<ILogStore>()
            .LogAsync(AppId, UserId, LogMessage.MobilePush_TokenAdded("System", UserId, Token.Token));

        return default;
    }
}
