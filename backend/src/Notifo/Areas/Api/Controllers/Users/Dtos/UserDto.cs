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
    public sealed class UserDto
    {
        /// <summary>
        /// The id of the user.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The unique api key for the user.
        /// </summary>
        public string ApiKey { get; set; }

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
        /// True when only whitelisted topic are allowed.
        /// </summary>
        public bool RequiresWhitelistedTopics { get; set; }

        /// <summary>
        /// Notification settings per channel.
        /// </summary>
        public NotificationSettingsDto Settings { get; set; }

        /// <summary>
        /// The statistics counters.
        /// </summary>
        public CounterMap Counters { get; set; }

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
