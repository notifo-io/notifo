// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Integrations.Resources;
using Notifo.Domain.Integrations.Smtp;
using Squidex.Hosting;

namespace Notifo.Domain.Integrations.AmazonSES
{
    public sealed class IntegratedAmazonSESIntegration : IIntegration, IInitializable
    {
        private readonly AmazonSESOptions options;
        private readonly SmtpEmailServer smtpEmailServer;
        private AmazonSimpleEmailServiceClient amazonSES;

        private static readonly IntegrationProperty FromEmailProperty = new IntegrationProperty("fromEmail", IntegrationPropertyType.Text)
        {
            Pattern = Patterns.Email,
            EditorLabel = Texts.AmazonSES_FromEmailLabel,
            EditorDescription = Texts.Email_FromEmailDescription,
            IsRequired = true,
            Summary = true
        };

        private static readonly IntegrationProperty FromNameProperty = new IntegrationProperty("fromName", IntegrationPropertyType.Text)
        {
            EditorLabel = Texts.AmazonSES_FromNameLabel,
            EditorDescription = Texts.Email_FromNameDescription,
            IsRequired = true
        };

        public IntegrationDefinition Definition { get; } =
            new IntegrationDefinition(
                "AmazonSES",
                Texts.AmazonSES_Name,
                "./integrations/amazon-ses.svg",
                new List<IntegrationProperty>
                {
                    FromEmailProperty,
                    FromNameProperty
                },
                new HashSet<string>
                {
                    Providers.Email
                })
            {
                Description = Texts.AmazonSES_Description
            };

        public IntegratedAmazonSESIntegration(IOptions<AmazonSESOptions> options)
        {
            this.options = options.Value;

            smtpEmailServer = new SmtpEmailServer(options.Value);
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

        public bool CanCreate(Type serviceType, ConfiguredIntegration configured)
        {
            return serviceType == typeof(IEmailSender);
        }

        public object? Create(Type serviceType, ConfiguredIntegration configured)
        {
            if (CanCreate(serviceType, configured))
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

        public async Task OnConfiguredAsync(ConfiguredIntegration configured, ConfiguredIntegration? previous)
        {
            var fromEmail = FromEmailProperty.GetString(configured);

            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                return;
            }

            var previousEmail = (string?)null;

            if (previous != null)
            {
                previousEmail = FromEmailProperty.GetString(previous);
            }

            if (string.Equals(previousEmail, fromEmail, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            await VerifyAsync(fromEmail, default);

            configured.Status = await GetStatusAsync(fromEmail, default);
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

        public async Task CheckStatusAsync(ConfiguredIntegration configured)
        {
            var fromEmail = FromEmailProperty.GetString(configured);

            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                return;
            }

            configured.Status = await GetStatusAsync(fromEmail, default);
        }

        private async Task<IntegrationStatus> GetStatusAsync(string emailAddress,
            CancellationToken ct)
        {
            var request = new GetIdentityVerificationAttributesRequest
            {
                Identities = new List<string> { emailAddress }
            };

            var response = await amazonSES.GetIdentityVerificationAttributesAsync(request, ct);

            var status = response.VerificationAttributes.FirstOrDefault(x => string.Equals(emailAddress, x.Key, StringComparison.OrdinalIgnoreCase));

            return MapStatus(status.Value.VerificationStatus);
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
    }
}
