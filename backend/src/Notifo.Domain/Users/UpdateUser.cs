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
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Users
{
    public sealed class UpdateUser : ICommand<User>
    {
        public string? FullName { get; set; }

        public string? EmailAddress { get; set; }

        public string? PhoneNumber { get; set; }

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

        public Task ExecuteAsync(User user, IServiceProvider serviceProvider, CancellationToken ct)
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
                user.ApiKey = RandomHash.New();
            }

            return Task.CompletedTask;
        }
    }
}
