// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.UserEvents;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Messaging;

namespace Notifo.Domain.Events.Pipeline
{
    public sealed class EventConsumer : IMessageHandler<EventMessage>
    {
        private readonly IUserEventPublisher userEventPublisher;

        public EventConsumer(IUserEventPublisher userEventPublisher)
        {
            this.userEventPublisher = userEventPublisher;
        }

        public async Task HandleAsync(EventMessage message,
            CancellationToken ct = default)
        {
            using (var trace = Telemetry.Activities.StartActivity("ConsumeEvent"))
            {
                await userEventPublisher.PublishAsync(message, ct);
            }
        }
    }
}
