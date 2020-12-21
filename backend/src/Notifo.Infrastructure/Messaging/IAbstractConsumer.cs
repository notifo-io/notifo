// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Infrastructure.Messaging
{
    public interface IAbstractConsumer<in T>
    {
        Task HandleAsync(T message, CancellationToken ct = default);
    }
}
