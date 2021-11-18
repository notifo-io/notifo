﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class UpsertUserDto
    {
        /// <summary>
        /// The id of the user.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// The full name of the user.
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// The email of the user.
        /// </summary>
        public string? EmailAddress { get; set; }

        /// <summary>
        /// The phone number.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// The threema id.
        /// </summary>
        public string? ThreemaId { get; set; }

        /// <summary>
        /// The telegram username.
        /// </summary>
        public string? TelegramUsername { get; set; }

        /// <summary>
        /// The telegram chat ID.
        /// </summary>
        public string? TelegramChatId { get; set; }

        /// <summary>
        /// The preferred language of the user.
        /// </summary>
        public string? PreferredLanguage { get; set; }

        /// <summary>
        /// The timezone of the user.
        /// </summary>
        public string? PreferredTimezone { get; set; }

        /// <summary>
        /// True when only whitelisted topic are allowed.
        /// </summary>
        public bool? RequiresWhitelistedTopics { get; set; }

        /// <summary>
        /// Notification settings per channel.
        /// </summary>
        public Dictionary<string, NotificationSettingDto>? Settings { get; set; }

        public UpdateUser ToUpdate()
        {
            var result = SimpleMapper.Map(this, new UpdateUser());

            if (Settings != null)
            {
                result.Settings = new NotificationSettings();

                foreach (var (key, value) in Settings)
                {
                    if (value != null)
                    {
                        result.Settings[key] = value.ToDomainObject();
                    }
                }
            }

            return result;
        }
    }
}
