// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Notifo.Domain.Apps;
using Notifo.Domain.Identity;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class AppDetailsDto
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
        public string[] Languages { get; set; }

        /// <summary>
        /// The confirm URL.
        /// </summary>
        public string? ConfirmUrl { get; set; }

        /// <summary>
        /// The api keys.
        /// </summary>
        [Required]
        public Dictionary<string, string> ApiKeys { get; set; }

        /// <summary>
        /// The contributors.
        /// </summary>
        [Required]
        public List<AppContributorDto> Contributors { get; set; }

        /// <summary>
        /// The statistics counters.
        /// </summary>
        [Required]
        public Dictionary<string, long> Counters { get; set; }

        public static async Task<AppDetailsDto> FromDomainObjectAsync(App app, string userId, IUserResolver userResolver)
        {
            var result = SimpleMapper.Map(app, new AppDetailsDto
            {
                Contributors = new List<AppContributorDto>()
            });

            if (userId != null && app.Contributors.TryGetValue(userId, out var userRole))
            {
                result.Role = userRole;
            }

            var users = await userResolver.QueryManyAsync(app.Contributors.Keys.Distinct().ToArray());

            foreach (var (id, role) in app.Contributors)
            {
                if (users.TryGetValue(id, out var user))
                {
                    result.Contributors.Add(new AppContributorDto
                    {
                        UserId = id,
                        UserName = user.Email,
                        Role = role
                    });
                }
            }

            result.Counters = app.Counters ?? EmptyCounters;

            return result;
        }
    }
}
