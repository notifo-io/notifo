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

public sealed class RemoveUserMobileToken : UserCommand
{
    public string Token { get; set; }

    private sealed class Validator : AbstractValidator<RemoveUserMobileToken>
    {
        public Validator()
        {
            RuleFor(x => x.Token).NotNull().NotEmpty();
        }
    }

    public override ValueTask<User?> ExecuteAsync(User target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        var token = Simplify(Token);

        if (target.MobilePushTokens.All(x => Simplify(x.Token) != token))
        {
            return default;
        }

        var newUser = target with
        {
            MobilePushTokens = target.MobilePushTokens.RemoveAll(x => Simplify(x.Token) == token)
        };

        return new ValueTask<User?>(newUser);
    }

    public override async ValueTask ExecutedAsync(IServiceProvider serviceProvider)
    {
        var logStore = serviceProvider.GetRequiredService<ILogStore>();

        await logStore.LogAsync(AppId, UserId, LogMessage.MobilePush_TokenRemoved("System", UserId, Token));
    }

    private static string Simplify(string url)
    {
        return Uri.UnescapeDataString(url);
    }
}
