// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Notifo.Infrastructure.Json;
using Notifo.Infrastructure.Tasks;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.Implementation.GooglePubSub
{
    public sealed class GooglePubSubMessaging<T> : IMessaging<T>
    {
        private readonly GooglePubSubOptions options;
        private readonly string topicId;
        private readonly IJsonSerializer serializer;
        private readonly ISemanticLog log;
        private PublisherClient? publisherClient;
        private SubscriberClient? subscriberClient;

        public int Order => 5000;

        public string Name => $"GooglePubSubMessaging({topicId})";

        public GooglePubSubMessaging(IOptions<GooglePubSubOptions> options, string topicId,
            IJsonSerializer serializer, ISemanticLog log)
        {
            this.options = options.Value;
            this.topicId = topicId;
            this.serializer = serializer;

            this.log = log;
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            var topicName = new TopicName(options.ProjectId, $"{options.Prefix}{topicId}");

            publisherClient = await PublisherClient.CreateAsync(topicName);
        }

        public async Task ReleaseAsync(CancellationToken ct)
        {
            if (subscriberClient != null)
            {
                await subscriberClient.StopAsync(ct);
            }
        }

        public async Task ProduceAsync(string key, Envelope<T> envelope,
            CancellationToken ct)
        {
            if (publisherClient == null)
            {
                throw new InvalidOperationException("Publisher not initialized yet.");
            }

            var serialized = serializer.SerializeToBytes(envelope);

            var pubSubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFrom(serialized)
            };

            await publisherClient.PublishAsync(pubSubMessage);
        }

        public Task SubscribeAsync(MessageCallback<T> onMessage,
            CancellationToken ct)
        {
            SubscribeAsync(onMessage).Forget();

            return Task.CompletedTask;
        }

        private async Task SubscribeAsync(MessageCallback<T> onMessage)
        {
            var subcriptionName = new SubscriptionName(options.ProjectId, $"{options.Prefix}{topicId}");

            subscriberClient = await SubscriberClient.CreateAsync(subcriptionName);
            subscriberClient.StartAsync(async (pubSubMessage, subscriberToken) =>
            {
                var message = serializer.Deserialize<Envelope<T>>(pubSubMessage.Data.Span);

                await onMessage(message, subscriberToken);

                return SubscriberClient.Reply.Ack;
            }).ContinueWith(task =>
            {
                var exception = task.Exception?.Flatten()?.InnerException;

                if (exception != null && exception is not OperationCanceledException)
                {
                    log.LogError(exception, w => w
                        .WriteProperty("action", "ConsumeMessage")
                        .WriteProperty("system", "GooglePubSub")
                        .WriteProperty("status", "Failed"));
                }
            }, CancellationToken.None).Forget();
        }
    }
}
