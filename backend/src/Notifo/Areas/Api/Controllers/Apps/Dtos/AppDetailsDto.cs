// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Counters;
using Notifo.Infrastructure.Identity;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class AppDetailsDto
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
        /// The sender email address.
        /// </summary>
        public string? EmailAddress { get; set; }

        /// <summary>
        /// The sender email name.
        /// </summary>
        public string? EmailName { get; set; }

        /// <summary>
        /// The firebase project ID.
        /// </summary>
        public string? FirebaseProject { get; set; }

        /// <summary>
        /// The firebase credentials.
        /// </summary>
        public string? FirebaseCredential { get; set; }

        /// <summary>
        /// The webhook URL.
        /// </summary>
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// The confirm URL.
        /// </summary>
        public string? ConfirmUrl { get; set; }

        /// <summary>
        /// True, when emails are allowed.
        /// </summary>
        public bool AllowEmail { get; set; }

        /// <summary>
        /// True, when SMS are allowed.
        /// </summary>
        public bool AllowSms { get; set; }

        /// <summary>
        /// The verification status of the email.
        /// </summary>
        public EmailVerificationStatus EmailVerificationStatus { get; set; }

        /// <summary>
        /// The api keys.
        /// </summary>
        public Dictionary<string, string> ApiKeys { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// The contributors.
        /// </summary>
        public List<AppContributorDto> Contributors { get; set; }

        /// <summary>
        /// The statistics counters.
        /// </summary>
        public CounterMap Counters { get; set; }

        public static async Task<AppDetailsDto> FromDomainObjectAsync(App app, string userId, IUserResolver userResolver)
        {
            var result = SimpleMapper.Map(app, new AppDetailsDto
            {
                Contributors = new List<AppContributorDto>()
            });

            if (app.ApiKeys != null)
            {
                foreach (var key in app.ApiKeys)
                {
                    result.ApiKeys[key.Key] = key.Role;
                }
            }

            if (userId != null && app.Contributors.TryGetValue(userId, out var userRole))
            {
                result.Role = userRole;
            }

            var users = await userResolver.GetUserNamesAsync(app.Contributors.Keys.ToHashSet());

            foreach (var (id, role) in app.Contributors)
            {
                if (users.TryGetValue(id, out var name))
                {
                    result.Contributors.Add(new AppContributorDto
                    {
                        UserId = id,
                        UserName = name,
                        Role = role
                    });
                }
            }

            result.Counters ??= new CounterMap();

            return result;
        }
    }
}
