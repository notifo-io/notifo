// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class ProfileDto
    {
        /// <summary>
        /// The full name of the user.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// The email of the user.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// The phone number.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The preferred language of the user.
        /// </summary>
        public string PreferredLanguage { get; set; }

        /// <summary>
        /// The timezone of the user.
        /// </summary>
        public string PreferredTimezone { get; set; }

        /// <summary>
        /// The supported languages.
        /// </summary>
        public string[] SupportedLanguages { get; set; }

        /// <summary>
        /// The supported timezones.
        /// </summary>
        public string[] SupportedTimezones { get; set; }

        /// <summary>
        /// Notification settings per channel.
        /// </summary>
        public Dictionary<string, NotificationSettingDto> Settings { get; set; }

        public static ProfileDto FromDomainObject(User user, App app)
        {
            var result = SimpleMapper.Map(user, new ProfileDto());

            result.SupportedTimezones = DateTimeZoneProviders.Tzdb.Ids.ToArray();
            result.SupportedLanguages = app.Languages;

            result.Settings ??= new Dictionary<string, NotificationSettingDto>();

            if (user.Settings != null)
            {
                foreach (var (key, value) in user.Settings)
                {
                    if (value != null)
                    {
                        result.Settings[key] = NotificationSettingDto.FromDomainObject(value);
                    }
                }
            }

            return result;
        }
    }
}
