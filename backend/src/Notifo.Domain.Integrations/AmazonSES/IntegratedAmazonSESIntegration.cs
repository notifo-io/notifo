// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Integrations.Resources;
using Notifo.Domain.Integrations.Smtp;
using Notifo.Infrastructure;
using Notifo.Infrastructure.KeyValueStore;
using Notifo.Infrastructure.Validation;
using Squidex.Hosting;

namespace Notifo.Domain.Integrations.AmazonSES
{
    public sealed class IntegratedAmazonSESIntegration : IIntegration, IInitializable
    {
        private readonly IKeyValueStore keyValueStore;
        private readonly AmazonSESOptions options;
        private readonly SmtpEmailServer smtpEmailServer;
        private AmazonSimpleEmailServiceClient amazonSES;

        private static readonly IntegrationProperty FromEmailProperty = new IntegrationProperty("fromEmail", PropertyType.Text)
        {
            Pattern = Patterns.Email,
            EditorLabel = Texts.Email_FromEmailLabel,
            EditorDescription = Texts.Email_FromEmailDescription,
            IsRequired = true,
            Summary = true
        };

        private static readonly IntegrationProperty FromNameProperty = new IntegrationProperty("fromName", PropertyType.Text)
        {
            EditorLabel = Texts.Email_FromNameLabel,
            EditorDescription = Texts.Email_FromNameDescription,
            IsRequired = true
        };

        private static readonly IntegrationProperty AdditionalFromEmails = new IntegrationProperty("additionalFromEmails", PropertyType.MultilineText)
        {
            EditorLabel = Texts.Email_AdditionalFromEmailsLabel,
            EditorDescription = Texts.Email_AdditionalFromEmailsDescription,
            IsRequired = false
        };

        public IntegrationDefinition Definition { get; } =
            new IntegrationDefinition(
                "AmazonSES",
                Texts.AmazonSES_Name,
                "./integrations/amazon-ses.svg",
                new List<IntegrationProperty>
                {
                    FromEmailProperty,
                    FromNameProperty,
                    AdditionalFromEmails
                },
                new List<UserProperty>(),
                new HashSet<string>
                {
                    Providers.Email
                })
            {
                Description = Texts.AmazonSES_Description
            };

        public IntegratedAmazonSESIntegration(IKeyValueStore keyValueStore, IOptions<AmazonSESOptions> options)
        {
            this.options = options.Value;

            smtpEmailServer = new SmtpEmailServer(options.Value);

            this.keyValueStore = keyValueStore;
        }

        public async Task InitializeAsync(
            CancellationToken ct)
        {
            amazonSES = new AmazonSimpleEmailServiceClient(
                options.AwsAccessKeyId,
                options.AwsSecretAccessKey,
                RegionEndpoint.GetBySystemName(options.Region));

            await amazonSES.GetSendQuotaAsync(ct);
        }

        public bool CanCreate(Type serviceType, string id, ConfiguredIntegration configured)
        {
            return serviceType == typeof(IEmailSender);
        }

        public object? Create(Type serviceType, string id, ConfiguredIntegration configured, IServiceProvider serviceProvider)
        {
            if (CanCreate(serviceType, id, configured))
            {
                var fromEmail = FromEmailProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(fromEmail))
                {
                    return null;
                }

                var fromName = FromNameProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(fromName))
                {
                    return null;
                }

                return new SmtpEmailSender(() => smtpEmailServer, fromEmail, fromName);
            }

            return null;
        }

        public async Task OnConfiguredAsync(App app, string id, ConfiguredIntegration configured, ConfiguredIntegration? previous,
            CancellationToken ct)
        {
            var fromEmails = GetEmailAddresses(configured).ToList();

            if (fromEmails.Count == 0)
            {
                return;
            }

            // Ensure that the email address is not used by another app.
            await ValidateEmailAddressesAsync(app, fromEmails, ct);

            var previousEmails = GetEmailAddresses(previous).ToList();

            if (previousEmails.SetEquals(fromEmails, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            // Remove unused email addresses to make them available for other apps.
            await CleanEmailsAsync(previousEmails.Except(fromEmails), ct);

            var unconfirmed = await GetUnconfirmedAsync(fromEmails, ct);

            // If all email addresses are already confirmed, we can use the integration.
            if (unconfirmed.Count == 0)
            {
                configured.Status = IntegrationStatus.Verified;
                return;
            }

            foreach (var email in unconfirmed)
            {
                await VerifyAsync(email, default);
            }

            configured.Status = IntegrationStatus.Pending;
        }

        public async Task OnRemovedAsync(App app, string id, ConfiguredIntegration configured,
            CancellationToken ct)
        {
            // Remove unused email addresses to make them available for other apps.
            await CleanEmailsAsync(GetEmailAddresses(configured), ct);
        }

        public async Task CheckStatusAsync(ConfiguredIntegration configured,
            CancellationToken ct)
        {
            // Check the status every few minutes to update the integration.
            configured.Status = await GetStatusAsync(GetEmailAddresses(configured).ToList(), ct);
        }

        private async Task ValidateEmailAddressesAsync(App app, List<string> fromEmails,
            CancellationToken ct)
        {
            if (!options.BindEmailAddresses)
            {
                return;
            }

            foreach (var email in fromEmails)
            {
                var key = StoreKey(email);

                if (await keyValueStore.SetIfNotExistsAsync(StoreKey(email), app.Id, ct) != app.Id)
                {
                    var error = string.Format(CultureInfo.InvariantCulture, Texts.AmazonSES_ReservedEmailAddress, email);

                    throw new ValidationException(error);
                }
            }
        }

        private async Task CleanEmailsAsync(IEnumerable<string> emails,
            CancellationToken ct)
        {
            foreach (var email in emails)
            {
                await amazonSES.DeleteIdentityAsync(new DeleteIdentityRequest
                {
                    Identity = email
                }, ct);

                await keyValueStore.RemvoveAsync(StoreKey(email), ct);
            }
        }

        private async Task VerifyAsync(string email,
            CancellationToken ct)
        {
            await amazonSES.DeleteIdentityAsync(new DeleteIdentityRequest
            {
                Identity = email
            }, ct);

            await amazonSES.VerifyEmailAddressAsync(new VerifyEmailAddressRequest
            {
                EmailAddress = email
            }, ct);
        }

        private async Task<IntegrationStatus> GetStatusAsync(List<string> fromEmails,
            CancellationToken ct)
        {
            var request = new GetIdentityVerificationAttributesRequest
            {
                Identities = fromEmails.ToList()
            };

            var response = await amazonSES.GetIdentityVerificationAttributesAsync(request, ct);

            var statuses = new List<IntegrationStatus>();

            foreach (var emailAddress in fromEmails)
            {
                var status = IntegrationStatus.Pending;

                if (response.VerificationAttributes.TryGetValue(emailAddress, out var result))
                {
                    status = MapStatus(result.VerificationStatus);
                }

                if (status == IntegrationStatus.VerificationFailed)
                {
                    return status;
                }

                statuses.Add(status);
            }

            if (statuses.All(x => x == IntegrationStatus.Verified))
            {
                return IntegrationStatus.Verified;
            }

            return IntegrationStatus.Pending;
        }

        private async Task<List<string>> GetUnconfirmedAsync(List<string> fromEmails,
            CancellationToken ct)
        {
            var request = new GetIdentityVerificationAttributesRequest
            {
                Identities = fromEmails.ToList()
            };

            var response = await amazonSES.GetIdentityVerificationAttributesAsync(request, ct);

            var result = new List<string>();

            foreach (var emailAddress in fromEmails)
            {
                if (!response.VerificationAttributes.TryGetValue(emailAddress, out var item) || item.VerificationStatus != VerificationStatus.Success)
                {
                    result.Add(emailAddress);
                }
            }

            return result;
        }

        private static IntegrationStatus MapStatus(VerificationStatus status)
        {
            if (status.Equals(VerificationStatus.NotStarted))
            {
                return IntegrationStatus.Pending;
            }

            if (status.Equals(VerificationStatus.Pending))
            {
                return IntegrationStatus.Pending;
            }

            if (status.Equals(VerificationStatus.Success))
            {
                return IntegrationStatus.Verified;
            }

            return IntegrationStatus.VerificationFailed;
        }

        private static IEnumerable<string> GetEmailAddresses(ConfiguredIntegration? configured)
        {
            if (configured == null)
            {
                yield break;
            }

            var hasAdded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var fromEmail = FromEmailProperty.GetString(configured);

            if (!string.IsNullOrWhiteSpace(fromEmail))
            {
                if (hasAdded.Add(fromEmail))
                {
                    yield return fromEmail;
                }
            }

            var additionalEmails = AdditionalFromEmails.GetString(configured);

            if (string.IsNullOrWhiteSpace(additionalEmails))
            {
                yield break;
            }

            var emails = additionalEmails.Split(new[] { '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var email in emails)
            {
                var trimmed = email.Trim().ToLowerInvariant();

                if (hasAdded.Add(trimmed))
                {
                    yield return trimmed;
                }
            }
        }

        private static string StoreKey(string email)
        {
            return $"{nameof(IntegratedAmazonSESIntegration)}_{email}";
        }
    }
}
