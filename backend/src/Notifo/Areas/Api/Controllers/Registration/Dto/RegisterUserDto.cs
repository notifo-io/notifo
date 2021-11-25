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

namespace Notifo.Areas.Api.Controllers.Registration.Dtos
{
    public sealed class RegisterUserDto
    {
        public bool CreateUser { get; set; }

        public string? EmailAddress { get; set; }

        public string? DisplayName { get; set; }

        public string? PreferredLanguage { get; set; }

        public string? PreferredTimezone { get; set; }

        public string[]? Topics { get; set; }

        public UpsertUser ToUpsert()
        {
            var result = SimpleMapper.Map(this, new UpsertUser());

            result.FullName = DisplayName;

            result.Settings = new NotificationSettings
            {
                [Providers.WebPush] = new NotificationSetting
                {
                    Send = NotificationSend.Send
                }
            };

            if (!string.IsNullOrWhiteSpace(result.EmailAddress))
            {
                result.Settings[Providers.Email] = new NotificationSetting
                {
                    Send = NotificationSend.Send
                };
            }

            return result;
        }
    }
}
