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

        var existing = target.MobilePushTokens.FirstOrDefault(x => x.Token == Token.Token);
        if (existing != null)
        {
            // A client can register the token again with the correct device type or identifier.
            if (existing.DeviceType == Token.DeviceType && existing.DeviceIdentifier == Token.DeviceIdentifier)
            {
                return default;
            }

            var updatedTokens = target.MobilePushTokens.Select(x => x.Token == Token.Token ? Token with { LastWakeup = x.LastWakeup } : x);

            return new ValueTask<User?>(target with
            {
                MobilePushTokens = updatedTokens.ToReadonlyList()
            });
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

    public override async ValueTask ExecutedAsync(IServiceProvider serviceProvider)
    {
        var logStore = serviceProvider.GetRequiredService<ILogStore>();

        await logStore.LogAsync(AppId, UserId, LogMessage.MobilePush_TokenAdded("System", UserId, Token.Token));
    }
}
