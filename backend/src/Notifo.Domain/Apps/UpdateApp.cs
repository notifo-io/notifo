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
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Identity;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Apps
{
    public sealed class UpdateApp : ICommand<App>
    {
        public string UserId { get; set; }

        public string? Name { get; set; }

        public string? EmailAddress { get; set; }

        public string? EmailName { get; set; }

        public string? FirebaseProject { get; set; }

        public string? FirebaseCredential { get; set; }

        public string? ConfirmUrl { get; set; }

        public string? WebhookUrl { get; set; }

        public bool? AllowEmail { get; set; }

        public bool? AllowSms { get; set; }

        public string[]? Languages { get; set; }

        public bool CanCreate => true;

        private sealed class Validator : AbstractValidator<UpdateApp>
        {
            public Validator()
            {
                RuleForEach(x => x.Languages).Language();
                RuleFor(x => x.EmailAddress).EmailAddress().Unless(x => string.IsNullOrWhiteSpace(x.EmailAddress));
                RuleFor(x => x.WebhookUrl).Url();
                RuleFor(x => x.ConfirmUrl).Url();
            }
        }

        public async Task<bool> ExecuteAsync(App app, IServiceProvider serviceProvider, CancellationToken ct)
        {
            Validate<Validator>.It(this);

            if (!string.Equals(EmailAddress, app.EmailAddress, StringComparison.OrdinalIgnoreCase))
            {
                var emailServer = serviceProvider.GetRequiredService<IEmailServer>()!;

                if (!string.IsNullOrWhiteSpace(app.EmailAddress))
                {
                    await emailServer.RemoveEmailAddressAsync(app.EmailAddress, ct);
                }

                if (!string.IsNullOrWhiteSpace(EmailAddress))
                {
                    var appRepository = serviceProvider.GetRequiredService<IAppRepository>();

                    var (existing, _) = await appRepository.GetByEmailAddressAsync(EmailAddress, ct);

                    if (existing != null)
                    {
                        throw new DomainException("The email address is already in use by another app.");
                    }

                    var status = await emailServer.AddEmailAddressAsync(EmailAddress, ct);

                    app.EmailVerificationStatus = status;
                }

                app.EmailAddress = EmailAddress ?? string.Empty;
            }

            if (Name != null)
            {
                app.Name = Name;
            }

            if (EmailName != null)
            {
                app.EmailName = EmailName;
            }

            if (FirebaseProject != null)
            {
                app.FirebaseProject = FirebaseProject;
            }

            if (FirebaseCredential != null)
            {
                app.FirebaseCredential = FirebaseCredential;
            }

            if (WebhookUrl != null)
            {
                app.WebhookUrl = WebhookUrl;
            }

            if (ConfirmUrl != null)
            {
                app.ConfirmUrl = ConfirmUrl;
            }

            if (AllowEmail != null)
            {
                app.AllowEmail = AllowEmail.Value;
            }

            if (AllowSms != null)
            {
                app.AllowSms = AllowSms.Value;
            }

            if (Languages != null)
            {
                app.Languages = Languages;
            }

            if (app.ApiKeys.Count == 0)
            {
                app.ApiKeys[RandomHash.New()] = NotifoRoles.AppAdmin;
                app.ApiKeys[RandomHash.New()] = NotifoRoles.AppAdmin;
                app.ApiKeys[RandomHash.New()] = NotifoRoles.AppWebManager;
                app.ApiKeys[RandomHash.New()] = NotifoRoles.AppWebManager;
            }

            if (app.Contributors.Count == 0 && !string.IsNullOrWhiteSpace(UserId))
            {
                app.Contributors[UserId] = NotifoRoles.AppOwner;
            }

            return true;
        }
    }
}
