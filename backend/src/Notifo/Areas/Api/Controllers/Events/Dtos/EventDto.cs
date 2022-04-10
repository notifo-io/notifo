// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Events;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Events.Dtos
{
    public sealed class EventDto
    {
        private static readonly Dictionary<string, long> EmptyCounters = new Dictionary<string, long>();
        private static readonly Dictionary<string, string> EmptyProperties = new Dictionary<string, string>();
        private static readonly Dictionary<string, ChannelSettingDto> EmptySettings = new Dictionary<string, ChannelSettingDto>();

        /// <summary>
        /// The id of the event.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// The topic path.
        /// </summary>
        [Required]
        public string Topic { get; set; }

        /// <summary>
        /// A custom id to identity the creator.
        /// </summary>
        public string? CreatorId { get; set; }

        /// <summary>
        /// The display name.
        /// </summary>
        [Required]
        public string DisplayName { get; set; }

        /// <summary>
        /// Additional user defined data.
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// The optional name of the Email template.
        /// </summary>
        public string? EmailTemplate { get; set; }

        /// <summary>
        /// The optional name of the SMS template.
        /// </summary>
        public string? SmsTemplate { get; set; }

        /// <summary>
        /// The time when the event has been created.
        /// </summary>
        [Required]
        public Instant Created { get; set; }

        /// <summary>
        /// The final formatting infos.
        /// </summary>
        [Required]
        public NotificationFormattingDto Formatting { get; set; }

        /// <summary>
        /// Notification settings per channel.
        /// </summary>
        [Required]
        public Dictionary<string, ChannelSettingDto> Settings { get; set; }

        /// <summary>
        /// User defined properties.
        /// </summary>
        [Required]
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// The scheduling options.
        /// </summary>
        public SchedulingDto? Scheduling { get; set; }

        /// <summary>
        /// The statistics counters.
        /// </summary>
        [Required]
        public Dictionary<string, long> Counters { get; set; }

        /// <summary>
        /// True when silent.
        /// </summary>
        [Required]
        public bool Silent { get; set; }

        /// <summary>
        /// The time to live in seconds.
        /// </summary>
        public int? TimeToLiveInSeconds { get; set; }

        public static EventDto FromDomainObject(Event source, App app)
        {
            var result = SimpleMapper.Map(source, new EventDto
            {
                Settings = new Dictionary<string, ChannelSettingDto>()
            });

            result.Properties = source.Properties ?? EmptyProperties;

            if (source.Formatting.Subject.TryGetValue(app.Language, out var subject))
            {
                result.DisplayName = subject;
            }
            else
            {
                result.DisplayName = source.Formatting.Subject.Values.FirstOrDefault() ?? string.Empty;
            }

            if (source.Formatting != null)
            {
                result.Formatting = NotificationFormattingDto.FromDomainObject(source.Formatting);
            }

            if (source.Scheduling != null)
            {
                result.Scheduling = SchedulingDto.FromDomainObject(source.Scheduling);
            }

            if (source.Settings?.Count > 0)
            {
                result.Settings = new Dictionary<string, ChannelSettingDto>();

                foreach (var (key, value) in source.Settings)
                {
                    if (value != null)
                    {
                        result.Settings[key] = ChannelSettingDto.FromDomainObject(value);
                    }
                }
            }
            else
            {
                result.Settings = EmptySettings;
            }

            result.Counters = source.Counters ?? EmptyCounters;

            return result;
        }
    }
}
