// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Events
{
    public sealed class Event
    {
        public string Id { get; set; }

        public string AppId { get; set; }

        public string Topic { get; set; }

        public string? CreatorId { get; set; }

        public string? TemplateCode { get; set; }

        public string? EmailTemplate { get; set; }

        public string? SmsTemplate { get; set; }

        public string? Data { get; set; }

        public Instant Created { get; set; }

        public NotificationFormatting<LocalizedText> Formatting { get; set; }

        public NotificationSettings Settings { get; set; } = new NotificationSettings();

        public NotificationProperties Properties { get; set; }

        public Scheduling? Scheduling { get; set; }

        public CounterMap? Counters { get; set; }

        public bool Silent { get; set; }

        public int? TimeToLiveInSeconds { get; set; }
    }
}
