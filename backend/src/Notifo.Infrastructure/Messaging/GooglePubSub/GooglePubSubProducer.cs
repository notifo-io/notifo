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
using Notifo.Infrastructure.Initialization;
using Notifo.Infrastructure.Json;

namespace Notifo.Infrastructure.Messaging.GooglePubSub
{
    public sealed class GooglePubSubProducer<T> : IAbstractProducer<T>, IInitializable
    {
        private readonly TopicName topicName;
        private readonly IJsonSerializer serializer;
        private PublisherClient publisherClient;

        public GooglePubSubProducer(IOptions<GooglePubSubOptions> options, string topicId,
            IJsonSerializer serializer)
        {
            this.serializer = serializer;

            topicName = new TopicName(options.Value.ProjectId, options.Value.Prefix + topicId);
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
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
