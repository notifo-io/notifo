// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure.Initialization;

namespace Notifo.Infrastructure.Messaging.GooglePubSub
{
    public sealed class GooglePubSubProvider : IMessagingProvider
    {
        public IInitializable GetConsumer<TConsumer, T>(IServiceProvider serviceProvider, string channelName) where TConsumer : IAbstractConsumer<T>
        {
            return ActivatorUtilities.CreateInstance<GooglePubSubConsumer<TConsumer, T>>(serviceProvider, channelName);
        }

        public IAbstractProducer<T> GetProducer<T>(IServiceProvider serviceProvider, string channelName)
        {
            return ActivatorUtilities.CreateInstance<GooglePubSubProducer<T>>(serviceProvider, channelName);
        }
    }
}
