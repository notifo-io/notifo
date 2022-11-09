// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.Users;

public sealed class UpdateMobileWakeupTime : ICommand<User>
{
    public string Token { get; set; }

    public Instant Timestamp { get; set; }

    public ValueTask<User?> ExecuteAsync(User user, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        var index = user.MobilePushTokens.IndexOf(x => x.Token == Token);

        if (index < 0)
        {
            return default;
        }

        var newTokens = new List<MobilePushToken>(user.MobilePushTokens);

        newTokens[index] = newTokens[index] with
        {
            LastWakeup = Timestamp
        };

        var newUser = user with
        {
            MobilePushTokens = newTokens.ToReadonlyList()
        };

        return new ValueTask<User?>(newUser);
    }
}
