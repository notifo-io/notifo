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
using Notifo.Infrastructure.Json;
using Notifo.Infrastructure.Tasks;
using Squidex.Hosting;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.GooglePubSub
{
    public sealed class GooglePubSubConsumer<TConsumer, T> : IInitializable where TConsumer : IAbstractConsumer<T>
    {
        private readonly GooglePubSubOptions options;
        private readonly string topicId;
        private readonly TConsumer consumer;
        private readonly IJsonSerializer serializer;
        private readonly ISemanticLog log;
        private SubscriberClient subscriberClient;

        public string Name => $"GooglePubSubConsumer({topicId})";

        public GooglePubSubConsumer(IOptions<GooglePubSubOptions> options, string topicId,
            TConsumer consumer, IJsonSerializer serializer, ISemanticLog log)
        {
            this.options = options.Value;
            this.topicId = topicId;
            this.consumer = consumer;
            this.serializer = serializer;

            this.log = log;
        }

        public async Task ReleaseAsync(CancellationToken ct)
        {
            if (subscriberClient != null)
            {
                await subscriberClient.StopAsync(ct);
            }
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            var subcriptionName = new SubscriptionName(options.ProjectId, $"{options.Prefix}{topicId}");

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
            }).Forget();
        }
    }
}
