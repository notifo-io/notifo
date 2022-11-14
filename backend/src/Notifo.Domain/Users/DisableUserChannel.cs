// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Users;

public sealed class DisableUserChannel : UserCommand
{
    public string Channel { get; set; }

    public override ValueTask<User?> ExecuteAsync(User target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        if (!target.Settings.TryGetValue(Channel, out var setting) || setting.Send != ChannelSend.Send)
        {
            return new ValueTask<User?>(target);
        }

        var newSetting = setting with
        {
            Send = ChannelSend.NotSending
        };

        var newSettings = new ChannelSettings(target.Settings)
        {
            [Channel] = newSetting
        };

        var newUser = target with
        {
            Settings = newSettings
        };

        return new ValueTask<User?>(newUser);
    }
}
