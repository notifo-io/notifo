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
    public sealed class RemoveUserAllowedTopic : ICommand<User>
    {
        public TopicId Prefix { get; set; }

        public Task ExecuteAsync(User user, IServiceProvider serviceProvider, CancellationToken ct)
        {
            user.AllowedTopics.Remove(Prefix);

            return Task.CompletedTask;
        }
    }
}
