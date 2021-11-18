// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Events;

namespace Notifo.Domain.UserEvents
{
    public interface IUserEventPublisher
    {
        Task PublishAsync(EventMessage @event,
            CancellationToken ct);
    }
}
