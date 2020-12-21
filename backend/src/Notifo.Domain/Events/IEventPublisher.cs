// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Notifo.Domain.Events
{
    public interface IEventPublisher
    {
        Task PublishAsync(EventMessage message);
    }
}
