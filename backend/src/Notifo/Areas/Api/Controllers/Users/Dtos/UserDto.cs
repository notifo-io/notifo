// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NodaTime;
using Notifo.Domain.Integrations;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Users.Dtos
{
    public sealed class UserDto
    {
        private static readonly Dictionary<string, long> EmptyCounters = new Dictionary<string, long>();

        /// <summary>
        /// The id of the user.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// The unique api key for the user.
        /// </summary>
        [Required]
        public string ApiKey { get; set; }

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
        /// The preferred language of the user.
        /// </summary>
        public string? PreferredLanguage { get; set; }

        /// <summary>
        /// The timezone of the user.
        /// </summary>
        public string? PreferredTimezone { get; set; }

        /// <summary>
        /// The date time (ISO 8601) when the user has been created.
        /// </summary>
        [Required]
        public Instant Created { get; set; }

        /// <summary>
        /// The date time (ISO 8601) when the user has been updated.
        /// </summary>
        [Required]
        public Instant LastUpdate { get; set; }

        /// <summary>
        /// The date time (ISO 8601) when the user has been received the last notification.
        /// </summary>
        public Instant? LastNotification { get; set; }

        /// <summary>
        /// True when only whitelisted topic are allowed.
        /// </summary>
        [Required]
        public bool RequiresWhitelistedTopics { get; set; }

        /// <summary>
        /// The user properties.
        /// </summary>
        public ReadonlyDictionary<string, string>? Properties { get; set; }

        /// <summary>
        /// Notification settings per channel.
        /// </summary>
        [Required]
        public Dictionary<string, ChannelSettingDto> Settings { get; set; } = new Dictionary<string, ChannelSettingDto>();

        /// <summary>
        /// The statistics counters.
        /// </summary>
        [Required]
        public Dictionary<string, long> Counters { get; set; }

        /// <summary>
        /// The mobile push tokens.
        /// </summary>
        [Required]
        public List<MobilePushTokenDto> MobilePushTokens { get; set; } = new List<MobilePushTokenDto>();

        /// <summary>
        /// The web push subscriptions.
        /// </summary>
        [Required]
        public List<WebPushSubscriptionDto> WebPushSubscriptions { get; set; } = new List<WebPushSubscriptionDto>();

        /// <summary>
        /// The supported user properties.
        /// </summary>
        public List<UserPropertyDto>? UserProperties { get; set; }

        public static UserDto FromDomainObject(User source, List<UserProperty>? properties, IReadOnlyDictionary<string, Instant>? lastNotifications)
        {
            var result = SimpleMapper.Map(source, new UserDto());

            if (source.Settings != null)
            {
                foreach (var (key, value) in source.Settings)
                {
                    if (value != null)
                    {
                        result.Settings[key] = ChannelSettingDto.FromDomainObject(value);
                    }
                }
            }

            foreach (var subscription in source.WebPushSubscriptions.OrEmpty())
            {
                result.WebPushSubscriptions.Add(WebPushSubscriptionDto.FromDomainObject(subscription));
            }

            foreach (var token in source.MobilePushTokens.OrEmpty())
            {
                result.MobilePushTokens.Add(MobilePushTokenDto.FromDomainObject(token));
            }

            if (properties != null)
            {
                result.UserProperties = properties.Select(UserPropertyDto.FromDomainObject).ToList();
            }

            result.Counters = source.Counters ?? EmptyCounters;

            if (lastNotifications != null && lastNotifications.TryGetValue(source.Id, out var lastNotification))
            {
                result.LastNotification = lastNotification;
            }

            return result;
        }
    }
}
