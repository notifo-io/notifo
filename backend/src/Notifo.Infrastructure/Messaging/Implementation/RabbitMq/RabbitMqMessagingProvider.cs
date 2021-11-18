// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Notifo.Infrastructure.Messaging.Implementation.RabbitMq
{
    public sealed class RabbitMqMessagingProvider : IMessagingProvider, IDisposable
    {
        public IConnection Connection { get; }

        public RabbitMqOptions Options { get; }

        public RabbitMqMessagingProvider(IOptions<RabbitMqOptions> options)
        {
            Options = options.Value;

            var factory = new ConnectionFactory
            {
                Uri = options.Value.Uri
            };

            factory.DispatchConsumersAsync = true;

            Connection = factory.CreateConnection();
        }

        public void Dispose()
        {
            Connection.Dispose();
        }

        public IMessaging<T> GetMessaging<T>(IServiceProvider serviceProvider, string channelName)
        {
            return ActivatorUtilities.CreateInstance<RabbitMqMessaging<T>>(serviceProvider, channelName);
        }
    }
}
