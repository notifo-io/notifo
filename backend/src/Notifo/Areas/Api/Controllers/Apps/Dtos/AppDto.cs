// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Notifo.Domain.Apps;
using Notifo.Domain.Counters;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class AppDto
    {
        /// <summary>
        /// The id of the app.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The app name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The current role.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// The supported languages.
        /// </summary>
        public string[] Languages { get; set; }

        /// <summary>
        /// The api keys.
        /// </summary>
        public Dictionary<string, string> ApiKeys { get; set; }

        /// <summary>
        /// The statistics counters.
        /// </summary>
        public CounterMap Counters { get; set; }

        public static AppDto FromDomainObject(App app, string userId)
        {
            var result = SimpleMapper.Map(app, new AppDto());

            if (userId != null && app.Contributors.TryGetValue(userId, out var userRole))
            {
                result.Role = userRole;
            }

            result.Counters ??= new CounterMap();

            return result;
        }
    }
}
