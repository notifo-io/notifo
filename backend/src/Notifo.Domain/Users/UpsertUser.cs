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

            if (FullName != null && !string.Equals(FullName, user.FullName, StringComparison.Ordinal))
            {
                newUser = newUser with
                {
                    FullName = FullName.Trim()
                };
            }

            if (EmailAddress != null && !string.Equals(EmailAddress, user.EmailAddress, StringComparison.OrdinalIgnoreCase))
            {
                newUser = newUser with
                {
                    EmailAddress = EmailAddress.Trim().ToLowerInvariant()
                };
            }

            if (PhoneNumber != null && !string.Equals(PhoneNumber, user.PhoneNumber, StringComparison.OrdinalIgnoreCase))
            {
                newUser = newUser with
                {
                    PhoneNumber = PhoneNumber.Trim().ToLowerInvariant()
                };
            }

            if (Properties != null && !Properties.Equals(user.Properties))
            {
                newUser = newUser with
                {
                    Properties = Properties
                };
            }

            if (PreferredLanguage != null && !string.Equals(PreferredLanguage, user.PreferredLanguage, StringComparison.Ordinal))
            {
                newUser = newUser with
                {
                    PreferredLanguage = PreferredLanguage.Trim()
                };
            }

            if (PreferredTimezone != null && !string.Equals(PreferredTimezone, user.PreferredTimezone, StringComparison.Ordinal))
            {
                newUser = newUser with
                {
                    PreferredTimezone = PreferredTimezone.Trim()
                };
            }

            if (RequiresWhitelistedTopics != null && RequiresWhitelistedTopics != user.RequiresWhitelistedTopics)
            {
                newUser = newUser with
                {
                    RequiresWhitelistedTopics = RequiresWhitelistedTopics.Value
                };
            }

            if (Settings != null)
            {
                newUser = newUser with
                {
                    Settings = new NotificationSettings(Settings)
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
