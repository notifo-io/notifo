// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.MobilePush;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.Users;

public sealed class UpdateMobileWakeupTime : UserCommand
{
    public string Token { get; set; }

    public override ValueTask<User?> ExecuteAsync(User target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        var index = target.MobilePushTokens.IndexOf(x => x.Token == Token);

        if (index < 0)
        {
            return default;
        }

        var newTokens = new List<MobilePushToken>(target.MobilePushTokens);

        newTokens[index] = newTokens[index] with
        {
            LastWakeup = Timestamp
        };

        var newUser = target with
        {
            MobilePushTokens = newTokens.ToReadonlyList()
        };

        return new ValueTask<User?>(newUser);
    }
}
