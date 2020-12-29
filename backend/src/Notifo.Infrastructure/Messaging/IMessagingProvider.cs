// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Hosting;

namespace Notifo.Infrastructure.Messaging
{
    public interface IMessagingProvider
    {
        IInitializable GetConsumer<TConsumer, T>(IServiceProvider serviceProvider, string channelName) where TConsumer : IAbstractConsumer<T>;

        IAbstractProducer<T> GetProducer<T>(IServiceProvider serviceProvider, string channelName);
    }
}
