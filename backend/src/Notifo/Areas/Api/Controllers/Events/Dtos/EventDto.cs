// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Counters;
using Notifo.Domain.Events;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Events.Dtos
{
    public sealed class EventDto
    {
        /// <summary>
        /// The id of the event.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The topic path.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// A custom id to identity the creator.
        /// </summary>
        public string? CreatorId { get; set; }

        /// <summary>
        /// The display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Additional user defined data.
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// The time when the event has been created.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The final formatting infos.
        /// </summary>
        public NotificationFormattingDto Formatting { get; set; }

        /// <summary>
        /// Notification settings per channel.
        /// </summary>
        public NotificationSettingsDto Settings { get; set; }

        /// <summary>
        /// User defined properties.
        /// </summary>
        public EventProperties Properties { get; set; }

        /// <summary>
        /// The scheduling options.
        /// </summary>
        public SchedulingDto? Scheduling { get; set; }

        /// <summary>
        /// The statistics counters.
        /// </summary>
        public CounterMap? Counters { get; set; }

        /// <summary>
        /// True when silent.
        /// </summary>
        public bool Silent { get; set; }

        public static EventDto FromDomainObject(Event source, App app)
        {
            var result = SimpleMapper.Map(source, new EventDto());

            if (source.Formatting.Subject.TryGetValue(app.Language, out var subject))
            {
                result.DisplayName = subject;
            }
            else
            {
                result.DisplayName = source.Formatting.Subject.Values.FirstOrDefault() ?? string.Empty;
            }

            result.Settings = new NotificationSettingsDto();

            if (source.Settings != null)
            {
                foreach (var (key, value) in source.Settings)
                {
                    if (value != null)
                    {
                        result.Settings[key] = NotificationSettingDto.FromDomainObject(value);
                    }
                }
            }

            if (result.Formatting == null)
            {
                result.Formatting = new NotificationFormattingDto();
            }

            if (result.Properties == null)
            {
                result.Properties = new EventProperties();
            }

            if (result.Counters == null)
            {
                result.Counters = new CounterMap();
            }

            return result;
        }
    }
}
