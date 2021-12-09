// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Mailjet
{
    public sealed class MailjetIntegration : IIntegration
    {
        private readonly MailjetEmailServerPool serverPool;

        private static readonly IntegrationProperty ApiKeyProperty = new IntegrationProperty("apiKey", IntegrationPropertyType.Password)
        {
            EditorLabel = Texts.Mailjet_ApiKeyLabel,
            EditorDescription = null,
            IsRequired = true
        };

        private static readonly IntegrationProperty ApiSecretProperty = new IntegrationProperty("apiSecret", IntegrationPropertyType.Password)
        {
            EditorLabel = Texts.Mailjet_ApiSecretLabel,
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
                "Mailjet",
                Texts.Mailjet_Name,
                "./integrations/mailjet.svg",
                new List<IntegrationProperty>
                {
                    ApiKeyProperty,
                    ApiSecretProperty,
                    FromEmailProperty,
                    FromNameProperty
                },
                new List<UserProperty>(),
                new HashSet<string>
                {
                    Providers.Email
                })
            {
                Description = Texts.Mailjet_Description
            };

        public MailjetIntegration(MailjetEmailServerPool serverPool)
        {
            this.serverPool = serverPool;
        }

        public bool CanCreate(Type serviceType, string id, ConfiguredIntegration configured)
        {
            return serviceType == typeof(IEmailSender);
        }

        public object? Create(Type serviceType, string id, ConfiguredIntegration configured, IServiceProvider serviceProvider)
        {
            if (CanCreate(serviceType, id, configured))
            {
                var publicKey = ApiKeyProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(publicKey))
                {
                    return null;
                }

                var privateKey = ApiSecretProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(privateKey))
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

                return new MailjetEmailSender(
                    () => serverPool.GetServer(publicKey, privateKey),
                    fromEmail,
                    fromName);
            }

            return null;
        }
    }
}
