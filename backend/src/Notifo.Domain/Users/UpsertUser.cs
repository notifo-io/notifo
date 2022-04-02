// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Users
{
    public sealed class UpsertUser : ICommand<User>
    {
        public string? FullName { get; set; }

        public string? EmailAddress { get; set; }

        public string? PhoneNumber { get; set; }

        public string? PreferredLanguage { get; set; }

        public string? PreferredTimezone { get; set; }

        public bool? RequiresWhitelistedTopics { get; set; }

        public ReadonlyDictionary<string, string>? Properties { get; set; }

        public NotificationSettings? Settings { get; set; }

        public bool MergeSettings { get; set; }

        public bool CanCreate => true;

        private sealed class Validator : AbstractValidator<UpsertUser>
        {
            public Validator()
            {
                RuleFor(x => x.EmailAddress).EmailAddress().Unless(x => string.IsNullOrWhiteSpace(x.EmailAddress));
                RuleFor(x => x.PreferredLanguage).Language();
                RuleFor(x => x.PreferredTimezone).Timezone();
                RuleFor(x => x.PhoneNumber).PhoneNumber();
            }
        }

        public async ValueTask<User?> ExecuteAsync(User user, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var newUser = user;

            if (Is.IsChanged(FullName, user.FullName))
            {
                newUser = newUser with
                {
                    FullName = FullName.Trim()
                };
            }

            if (Is.IsChanged(EmailAddress, user.EmailAddress))
            {
                newUser = newUser with
                {
                    EmailAddress = EmailAddress.Trim().ToLowerInvariant()
                };
            }

            if (Is.IsChanged(PhoneNumber, user.PhoneNumber))
            {
                newUser = newUser with
                {
                    PhoneNumber = PhoneNumber.Trim().ToLowerInvariant()
                };
            }

            if (Is.IsChanged(Properties, user.Properties))
            {
                newUser = newUser with
                {
                    Properties = Properties
                };
            }

            if (Is.IsChanged(PreferredLanguage, user.PreferredLanguage))
            {
                newUser = newUser with
                {
                    PreferredLanguage = PreferredLanguage.Trim()
                };
            }

            if (Is.IsChanged(PreferredTimezone, user.PreferredTimezone))
            {
                newUser = newUser with
                {
                    PreferredTimezone = PreferredTimezone.Trim()
                };
            }

            if (Is.IsChanged(RequiresWhitelistedTopics, user.RequiresWhitelistedTopics))
            {
                newUser = newUser with
                {
                    RequiresWhitelistedTopics = RequiresWhitelistedTopics.Value
                };
            }

            if (Settings != null)
            {
                var newSettings = Settings;

                if (MergeSettings)
                {
                    newSettings = NotificationSettings.Merged(user.Settings, Settings);
                }

                newUser = newUser with
                {
                    Settings = newSettings
                };
            }

            if (string.IsNullOrWhiteSpace(user.ApiKey))
            {
                var tokenGenerator = serviceProvider.GetRequiredService<IApiKeyGenerator>();

                newUser = newUser with
                {
                    ApiKey = await tokenGenerator.GenerateUserTokenAsync(user.AppId, user.Id)
                };
            }

            return newUser;
        }
    }
}
