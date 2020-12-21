// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Events;

namespace Notifo.Domain.UserEvents
{
    public interface IUserEventPublisher
    {
        Task PublishAsync(EventMessage message, CancellationToken ct);
    }
}
