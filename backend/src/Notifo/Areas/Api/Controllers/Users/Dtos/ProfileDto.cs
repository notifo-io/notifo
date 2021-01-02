// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;
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
        public NotificationSettingsDto Settings { get; set; }

        public static UserDto FromDomainObject(User user)
        {
            var result = SimpleMapper.Map(user, new UserDto());

            if (user.Settings != null)
            {
                result.Settings = new NotificationSettingsDto();

                foreach (var (key, value) in user.Settings)
                {
                    if (value != null)
                    {
                        result.Settings[key] = NotificationSettingDto.FromDomainObject(value);
                    }
                }
            }
            else
            {
                result.Settings = new NotificationSettingsDto();
            }

            if (result.Counters == null)
            {
                result.Counters = new CounterMap();
            }

            return result;
        }
    }
}
