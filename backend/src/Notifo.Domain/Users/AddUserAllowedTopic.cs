// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.Users
{
    public sealed class AddUserAllowedTopic : ICommand<User>
    {
        public TopicId Prefix { get; set; }

        public ValueTask<User?> ExecuteAsync(User user, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            if (user.AllowedTopics.Contains(Prefix))
            {
                return default;
            }

            var newAllowedTopics = new List<string>(user.AllowedTopics)
            {
                Prefix
            };

            var newUser = user with
            {
                AllowedTopics = newAllowedTopics.ToReadonlyList()
            };

            return new ValueTask<User?>(newUser);
        }
    }
}
