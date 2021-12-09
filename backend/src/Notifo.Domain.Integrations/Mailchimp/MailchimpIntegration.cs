// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Mailchimp
{
    public sealed class MailchimpIntegration : IIntegration
    {
        private static readonly IntegrationProperty ApiKeyProperty = new IntegrationProperty("apiKey", IntegrationPropertyType.Password)
        {
            EditorLabel = Texts.Mailchimp_ApiKeyLabel,
            EditorDescription = null,
            IsRequired = true
        };

        private static readonly IntegrationProperty FromEmailProperty = new IntegrationProperty("fromEmail", IntegrationPropertyType.Text)
        {
            Pattern = Patterns.Email,
            EditorLabel = Texts.Email_FromEmailLabel,
            EditorDescription = Texts.Email_FromEmailDescription,
            IsRequired = true,
            Summary = true
        };

        private static readonly IntegrationProperty FromNameProperty = new IntegrationProperty("fromName", IntegrationPropertyType.Text)
        {
            EditorLabel = Texts.Email_FromNameLabel,
            EditorDescription = Texts.Email_FromNameDescription,
            IsRequired = true
        };

        public IntegrationDefinition Definition { get; }
            = new IntegrationDefinition(
                "Mailchimp",
                Texts.Mailchimp_Name,
                "./integrations/mailchimp.svg",
                new List<IntegrationProperty>
                {
                    ApiKeyProperty,
                    FromEmailProperty,
                    FromNameProperty
                },
                new List<UserProperty>(),
                new HashSet<string>
                {
                    Providers.Email
                })
            {
                Description = Texts.Mailchimp_Description
            };

        public bool CanCreate(Type serviceType, string id, ConfiguredIntegration configured)
        {
            return serviceType == typeof(IEmailSender);
        }

        public object? Create(Type serviceType, string id, ConfiguredIntegration configured, IServiceProvider serviceProvider)
        {
            if (CanCreate(serviceType, id, configured))
            {
                var apiKey = ApiKeyProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return null;
                }

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

                return new MailchimpEmailSender(
                    serviceProvider.GetRequiredService<IHttpClientFactory>(),
                    apiKey,
                    fromEmail,
                    fromName);
            }

            return null;
        }
    }
}
