// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Notifo.Infrastructure.Json;
using Squidex.Hosting;

namespace Notifo.Infrastructure.Messaging.GooglePubSub
{
    public sealed class GooglePubSubProducer<T> : IAbstractProducer<T>, IInitializable
    {
        private readonly GooglePubSubOptions options;
        private readonly string topicId;
        private readonly IJsonSerializer serializer;
        private PublisherClient publisherClient;

        public string Name => $"GooglePubSubProducer({topicId})";

        public GooglePubSubProducer(IOptions<GooglePubSubOptions> options, string topicId,
            IJsonSerializer serializer)
        {
            this.options = options.Value;
            this.topicId = topicId;
            this.serializer = serializer;
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            var topicName = new TopicName(options.ProjectId, $"{options.Prefix}{topicId}");

            publisherClient = await PublisherClient.CreateAsync(topicName);
        }

        public async Task ProduceAsync(string key, T message)
        {
            var serialized = serializer.SerializeToBytes(message);

            var pubSubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFrom(serialized)
            };

            await publisherClient.PublishAsync(pubSubMessage);
        }
    }
}
