// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;

namespace Notifo.Domain.Users
{
    public sealed class AddUserAllowedTopic : ICommand<User>
    {
        public TopicId Prefix { get; set; }

        public Task<bool> ExecuteAsync(User user, IServiceProvider serviceProvider, CancellationToken ct)
        {
            if (!user.AllowedTopics.Contains(Prefix))
            {
                user.AllowedTopics.Add(Prefix);

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
