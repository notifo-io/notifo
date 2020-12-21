// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain;
using Notifo.Domain.Channels;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Registration.Dto
{
    public sealed class RegisterRequestDto
    {
        public bool CreateUser { get; set; }

        public string? EmailAddress { get; set; }

        public string? DisplayName { get; set; }

        public string? PreferredLanguage { get; set; }

        public string? PreferredTimezone { get; set; }

        public string[]? Topics { get; set; }

        public UpdateUser ToUpdate()
        {
            var result = SimpleMapper.Map(this, new UpdateUser());

            result.FullName = DisplayName;

            result.Settings = new NotificationSettings
            {
                [Providers.WebPush] = new NotificationSetting
                {
                    Send = true
                }
            };

            if (!string.IsNullOrWhiteSpace(result.EmailAddress))
            {
                result.Settings[Providers.Email] = new NotificationSetting
                {
                    Send = true
                };
            }

            return result;
        }
    }
}
