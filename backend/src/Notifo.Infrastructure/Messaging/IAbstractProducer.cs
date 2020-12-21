// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Notifo.Infrastructure.Messaging
{
    public interface IAbstractProducer<in T>
    {
        Task ProduceAsync(string key, T message);
    }
}