// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;
using Notifo.Infrastructure.Initialization;
using Notifo.Infrastructure.Json;
using Notifo.Infrastructure.Tasks;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.GooglePubSub
{
    public sealed class GooglePubSubConsumer<TConsumer, T> : IInitializable where TConsumer : IAbstractConsumer<T>
    {
        private readonly SubscriptionName subcriptionName;
        private readonly TConsumer consumer;
        private readonly IJsonSerializer serializer;
        private readonly ISemanticLog log;
        private SubscriberClient subscriberClient;

        public GooglePubSubConsumer(IOptions<GooglePubSubOptions> options, string topicId,
            TConsumer consumer, IJsonSerializer serializer, ISemanticLog log)
        {
            this.consumer = consumer;
            this.serializer = serializer;

            this.log = log;

            subcriptionName = new SubscriptionName(options.Value.ProjectId, options.Value.Prefix + topicId);
        }

        public async Task ReleaseAsync(CancellationToken ct = default)
        {
            if (subscriberClient != null)
            {
                await subscriberClient.StopAsync(ct);
            }
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            subscriberClient = await SubscriberClient.CreateAsync(subcriptionName);

            subscriberClient.StartAsync(async (pubSubMessage, subscriberToken) =>
            {
                try
                {
                    var message = serializer.Deserialize<T>(pubSubMessage.Data.Span);

                    await consumer.HandleAsync(message, subscriberToken);
                }
                catch (JsonException ex)
                {
                    log.LogError(ex, w => w
                        .WriteProperty("action", "ConsumeMessage")
                        .WriteProperty("system", "GooglePubSub")
                        .WriteProperty("status", "Failed")
                        .WriteProperty("reason", "Failed to deserialize from JSON."));
                }
                catch (Exception ex)
                {
                    log.LogError(ex, w => w
                        .WriteProperty("action", "ConsumeMessage")
                        .WriteProperty("system", "GooglePubSub")
                        .WriteProperty("status", "Failed"));
                }

                return SubscriberClient.Reply.Ack;
            }).Forget();
        }
    }
}
