// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using NodaTime;
using Notifo.Domain;
using Notifo.Domain.Events;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Events.Dtos
{
    public class PublishRequestDto
    {
        /// <summary>
        /// The topic path.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// A custom id to identity the creator.
        /// </summary>
        public string? CreatorId { get; set; }

        /// <summary>
        /// The template code.
        /// </summary>
        public string? TemplateCode { get; set; }

        /// <summary>
        /// Additional user defined data.
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// A custom timestamp.
        /// </summary>
        public Instant Timestamp { get; set; }

        /// <summary>
        /// Preformatting when no template is used.
        /// </summary>
        public NotificationFormattingDto? Preformatted { get; set; }

        /// <summary>
        /// The notification settings.
        /// </summary>
        public Dictionary<string, NotificationSettingDto>? Settings { get; set; }

        /// <summary>
        /// User defined properties.
        /// </summary>
        public EventProperties? Properties { get; set; }

        /// <summary>
        /// The scheduling options.
        /// </summary>
        public SchedulingDto? Scheduling { get; set; }

        /// <summary>
        /// True when silent.
        /// </summary>
        public bool Silent { get; set; }

        public EventMessage ToEvent(string appId)
        {
            var result = SimpleMapper.Map(this, new EventMessage());

            if (Preformatted != null)
            {
                result.Formatting = Preformatted.ToDomainObject();
            }

            if (Settings != null)
            {
                foreach (var (key, value) in Settings)
                {
                    if (value != null)
                    {
                        result.Settings[key] = value.ToDomainObject();
                    }
                }
            }

            if (Scheduling != null)
            {
                result.Scheduling = Scheduling.ToDomainObject();
            }
            else
            {
                result.Scheduling = new Scheduling();
            }

            if (result.Properties == null)
            {
                result.Properties = new EventProperties();
            }

            result.AppId = appId;

            return result;
        }
    }
}
