// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.Users;

public sealed class RemoveUserAllowedTopic : UserCommand
{
    public TopicId Prefix { get; set; }

    public override ValueTask<User?> ExecuteAsync(User target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        if (!target.AllowedTopics.Contains(Prefix))
        {
            return default;
        }

        var newUser = target with
        {
            AllowedTopics = target.AllowedTopics.Remove(Prefix)
        };

        return new ValueTask<User?>(newUser);
    }
}
