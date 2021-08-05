// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Users;

namespace Notifo.Domain.Channels.Messaging
{
    public interface IMessagingSender
    {
        bool HasTarget(User user);

        Task AddTargetsAsync(MessagingJob job, User user);

        Task<bool> SendAsync(MessagingJob job,
            CancellationToken ct);
    }
}
