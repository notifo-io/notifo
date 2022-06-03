// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifo.Infrastructure.Json;
using Notifo.Infrastructure.Tasks;

namespace Notifo.Infrastructure.Messaging.Implementation.GooglePubSub
{
    public sealed class GooglePubSubMessaging<T> : IMessaging<T>
    {
        private readonly GooglePubSubOptions options;
        private readonly string topicId;
        private readonly IJsonSerializer serializer;
        private readonly ILogger<GooglePubSubMessaging<T>> log;
        private PublisherClient? publisherClient;
        private SubscriberClient? subscriberClient;

        public int Order => 5000;

        public string Name => $"GooglePubSubMessaging({topicId})";

        public GooglePubSubMessaging(IOptions<GooglePubSubOptions> options, string topicId,
            IJsonSerializer serializer, ILogger<GooglePubSubMessaging<T>> log)
        {
            this.options = options.Value;
            this.topicId = topicId;
            this.serializer = serializer;

            this.log = log;
        }

        public async Task InitializeAsync(
            CancellationToken ct)
        {
            var topicName = new TopicName(options.ProjectId, $"{options.Prefix}{topicId}");

            publisherClient = await PublisherClient.CreateAsync(topicName);
        }

        public async Task ReleaseAsync(
            CancellationToken ct)
        {
            if (subscriberClient != null)
            {
                await subscriberClient.StopAsync(ct);
            }
        }

        public async Task ProduceAsync(string key, Envelope<T> envelope,
            CancellationToken ct = default)
        {
            if (publisherClient == null)
            {
                ThrowHelper.InvalidOperationException("Publisher not initialized yet.");
                return;
            }

            var serialized = serializer.SerializeToBytes(envelope);

            var pubSubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFrom(serialized)
            };

            await publisherClient.PublishAsync(pubSubMessage);
        }

        public async Task SubscribeAsync(MessageCallback<T> onMessage,
            CancellationToken ct = default)
        {
            var subcriptionName = new SubscriptionName(options.ProjectId, $"{options.Prefix}{topicId}");

            subscriberClient = await SubscriberClient.CreateAsync(subcriptionName);
            SubscribeCoreAsync(onMessage).Forget();
        }

        private async Task SubscribeCoreAsync(MessageCallback<T> onMessage)
        {
            try
            {
                await subscriberClient!.StartAsync(async (pubSubMessage, subscriberToken) =>
                {
                    var message = serializer.Deserialize<Envelope<T>>(pubSubMessage.Data.Span);

                    await onMessage(message, subscriberToken);

                    return SubscriberClient.Reply.Ack;
                });
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.LogError(ex, "Failed to consume message.");
            }
        }
    }
}
