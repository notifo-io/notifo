// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Notifo.Infrastructure.Initialization;

namespace Notifo.Infrastructure.Messaging
{
    public interface IAbstractProducer<in T> : IInitializable
    {
        Task ProduceAsync(string key, T message);
    }
}