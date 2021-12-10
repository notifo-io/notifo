// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using NodaTime;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.UserEvents
{
    public sealed class UserEventMessage
    {
        public string AppId { get; set; }

        public string UserId { get; set; }

        public string SubscriptionPrefix { get; set; }

        public string Topic { get; set; }

        public string EventId { get; set; }

        public string? Data { get; set; }

        public bool Silent { get; set; }

        public bool Test { get; set; }

        public int? TimeToLiveInSeconds { get; set; }

        public ActivityContext UserEventActivity { get; set; }

        public ActivityContext EventActivity { get; set; }

        public Instant Created { get; set; }

        public Instant Enqueued { get; set; }

        public NotificationSettings? EventSettings { get; set; }

        public NotificationSettings? SubscriptionSettings { get; set; }

        public NotificationFormatting<LocalizedText> Formatting { get; set; }

        public NotificationProperties Properties { get; set; }

        public Scheduling? Scheduling { get; set; }

        public override string ToString()
        {
            return Formatting.ToDebugString();
        }
    }
}
