// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Apps;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class AppDto
    {
        private static readonly Dictionary<string, long> EmptyCounters = new Dictionary<string, long>();

        /// <summary>
        /// The id of the app.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// The app name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The current role.
        /// </summary>
        [Required]
        public string Role { get; set; }

        /// <summary>
        /// The supported languages.
        /// </summary>
        [Required]
        public ReadonlyList<string> Languages { get; set; }

        /// <summary>
        /// The api keys.
        /// </summary>
        [Required]
        public ReadonlyDictionary<string, string> ApiKeys { get; set; }

        /// <summary>
        /// The statistics counters.
        /// </summary>
        [Required]
        public Dictionary<string, long> Counters { get; set; }

        public static AppDto FromDomainObject(App source, string userId)
        {
            var result = SimpleMapper.Map(source, new AppDto());

            if (userId != null && source.Contributors.TryGetValue(userId, out var userRole))
            {
                result.Role = userRole;
            }

            result.Counters = source.Counters ?? EmptyCounters;

            return result;
        }
    }
}
