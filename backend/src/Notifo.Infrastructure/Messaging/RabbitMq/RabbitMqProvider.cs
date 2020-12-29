// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Squidex.Hosting;

namespace Notifo.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqProvider : IMessagingProvider, IDisposable
    {
        private readonly IConnection connection;

        public IConnection Connection
        {
            get { return connection; }
        }

        public RabbitMqProvider(IOptions<RabbitMqOptions> options)
        {
            var factory = new ConnectionFactory
            {
                Uri = options.Value.Uri
            };

            factory.DispatchConsumersAsync = true;

            connection = factory.CreateConnection();
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public IInitializable GetConsumer<TConsumer, T>(IServiceProvider serviceProvider, string channelName) where TConsumer : IAbstractConsumer<T>
        {
            return ActivatorUtilities.CreateInstance<RabbitMqConsumer<T>>(serviceProvider, channelName);
        }

        public IAbstractProducer<T> GetProducer<T>(IServiceProvider serviceProvider, string channelName)
        {
            return ActivatorUtilities.CreateInstance<RabbitMqProducer<T>>(serviceProvider, channelName);
        }
    }
}
