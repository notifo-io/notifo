// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Squidex.Hosting;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqPrimitiveProducer : IInitializable, IDisposable
    {
        private readonly ConcurrentDictionary<ulong, (string Queue, byte[] Message)> outstandingConfirms = new ConcurrentDictionary<ulong, (string Queue, byte[] Message)>();
        private readonly BlockingCollection<(string Queue, byte[] Message)> retryQueue = new BlockingCollection<(string Queue, byte[] Message)>(5000);
        private readonly RabbitMqProvider connection;
        private readonly ISemanticLog log;
        private readonly Thread retryThread;
        private IModel channel;

        public int Order => -1;

        public RabbitMqPrimitiveProducer(RabbitMqProvider connection, ISemanticLog log)
        {
            this.connection = connection;

            this.log = log;

            retryThread = new Thread(Retry)
            {
                IsBackground = true
            };
        }

        public void Dispose()
        {
            channel?.Dispose();

            retryQueue.CompleteAdding();

            try
            {
                retryThread.Join(1500);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "Shutdown")
                    .WriteProperty("system", "RabbitMq")
                    .WriteProperty("status", "Failed"));
            }
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            channel = connection.Connection.CreateModel();

            channel.BasicAcks += (sender, @event) =>
            {
                CleanOutstanding(@event.DeliveryTag, @event.Multiple);
            };

            channel.BasicNacks += (sender, @event) =>
            {
                CleanOutstanding(@event.DeliveryTag, @event.Multiple, found =>
                {
                    if (!retryQueue.TryAdd(found, 2000))
                    {
                        log.LogError(w => w
                            .WriteProperty("action", "Shutdown")
                            .WriteProperty("system", "Kafka")
                            .WriteProperty("status", "Failed")
                            .WriteProperty("reason", "Queue full."));
                    }
                });
            };

            retryThread.Start();

            return Task.CompletedTask;
        }

        private void Retry()
        {
            try
            {
                foreach (var (queueName, message) in retryQueue.GetConsumingEnumerable())
                {
                    Publish(queueName, message);
                }
            }
            catch
            {
                try
                {
                    retryQueue.CompleteAdding();
                }
                catch
                {
                    return;
                }
            }
        }

        public void CreateQueue(string queueName)
        {
            lock (channel)
            {
                channel.QueueDeclare(queueName, true, false, false, null);
            }
        }

        public void Publish(string queueName, byte[] bytes)
        {
            lock (channel)
            {
                outstandingConfirms.TryAdd(channel.NextPublishSeqNo, (queueName, bytes));

                var props = channel.CreateBasicProperties();

                channel.BasicPublish(string.Empty, queueName, true, props, bytes);
            }
        }

        private void CleanOutstanding(ulong sequenceNumber, bool multiple, Action<(string Queue, byte[] Message)>? handler = null)
        {
            if (multiple)
            {
                var confirmed = outstandingConfirms.Where(k => k.Key <= sequenceNumber);

                foreach (var entry in confirmed)
                {
                    if (outstandingConfirms.TryRemove(entry.Key, out var found))
                    {
                        handler?.Invoke(found);
                    }
                }
            }
            else
            {
                if (outstandingConfirms.TryRemove(sequenceNumber, out var found))
                {
                    handler?.Invoke(found);
                }
            }
        }
    }
}
