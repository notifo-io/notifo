// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Squidex.Hosting;
using Squidex.Log;

namespace Notifo.Infrastructure.Messaging.Implementation
{
    public sealed class DelegatingConsumer<T> : IInitializable
    {
        private readonly IMessaging<T> messaging;
        private readonly IEnumerable<IMessageHandler<T>> messageHandlers;
        private readonly ISemanticLog log;
        private readonly string activity = $"Messaging.Consume({typeof(T).Name})";

        public string Name => $"MessagingConsumer({typeof(T).Name})";

        public int Order => int.MaxValue;

        public DelegatingConsumer(IMessaging<T> messaging, IEnumerable<IMessageHandler<T>> messageHandlers, ISemanticLog log)
        {
            this.messaging = messaging;
            this.messageHandlers = messageHandlers;

            this.log = log;
        }

        public async Task InitializeAsync(
            CancellationToken ct)
        {
            if (messaging is IInitializable initializable)
            {
                await initializable.InitializeAsync(ct);
            }

            if (messageHandlers.Any())
            {
                await messaging.SubscribeAsync(OnMessageAsync, ct);
            }
        }

        public async Task ReleaseAsync(
            CancellationToken ct)
        {
            if (messaging is IInitializable initializable)
            {
                await initializable.ReleaseAsync(ct);
            }
        }

        private async Task OnMessageAsync(Envelope<T> message,
            CancellationToken ct)
        {
            using (var trace = Telemetry.Activities.StartActivity(activity))
            {
                if (message.Created != default && trace?.Id != null)
                {
                    Telemetry.Activities.StartActivity("QueueTime", ActivityKind.Internal, trace.Id,
                        startTime: message.Created.ToDateTimeOffset())?.Stop();
                }

                foreach (var handler in messageHandlers)
                {
                    try
                    {
                        await handler.HandleAsync(message.Payload, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, w => w
                            .WriteProperty("action", "ConsumeMessage")
                            .WriteProperty("system", messaging.ToString())
                            .WriteProperty("status", "Failed"));
                    }
                }
            }
        }
    }
}
