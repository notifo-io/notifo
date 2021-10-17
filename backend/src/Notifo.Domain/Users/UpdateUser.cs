// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Users
{
    public sealed class UpdateUser : ICommand<User>
    {
        public string? FullName { get; set; }

        public string? EmailAddress { get; set; }

        public string? PhoneNumber { get; set; }

        public string? ThreemaId { get; set; }

        public string? TelegramChatId { get; set; }

        public string? TelegramUsername { get; set; }

        public string? PreferredLanguage { get; set; }

        public string? PreferredTimezone { get; set; }

        public bool? RequiresWhitelistedTopics { get; set; }

        public NotificationSettings? Settings { get; set; }

        public bool CanCreate => true;

        private sealed class Validator : AbstractValidator<UpdateUser>
        {
            public Validator()
            {
                RuleFor(x => x.EmailAddress).EmailAddress().Unless(x => string.IsNullOrWhiteSpace(x.EmailAddress));
                RuleFor(x => x.PreferredLanguage).Language();
                RuleFor(x => x.PreferredTimezone).Timezone();
                RuleFor(x => x.PhoneNumber).PhoneNumber();
            }
        }

        public async Task<bool> ExecuteAsync(User user, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            if (FullName != null)
            {
                user.FullName = FullName;
            }

            if (EmailAddress != null)
            {
                user.EmailAddress = EmailAddress;
            }

            if (PhoneNumber != null)
            {
                user.PhoneNumber = PhoneNumber;
            }

            if (TelegramChatId != null)
            {
                user.TelegramChatId = TelegramChatId;
            }

            if (TelegramUsername != null)
            {
                user.TelegramUsername = TelegramUsername;
            }

            if (ThreemaId != null)
            {
                user.ThreemaId = ThreemaId;
            }

            if (PreferredLanguage != null)
            {
                user.PreferredLanguage = PreferredLanguage;
            }

            if (PreferredTimezone != null)
            {
                user.PreferredTimezone = PreferredTimezone;
            }

            if (RequiresWhitelistedTopics != null)
            {
                user.RequiresWhitelistedTopics = RequiresWhitelistedTopics.Value;
            }

            if (Settings != null)
            {
                user.Settings ??= new NotificationSettings();

                foreach (var (key, value) in Settings)
                {
                    user.Settings[key] = value;
                }
            }

            if (string.IsNullOrWhiteSpace(user.ApiKey))
            {
                var tokenGenerator = serviceProvider.GetRequiredService<IApiKeyGenerator>();

                user.ApiKey = await tokenGenerator.GenerateUserTokenAsync(user.AppId, user.Id);
            }

            return true;
        }
    }
}
