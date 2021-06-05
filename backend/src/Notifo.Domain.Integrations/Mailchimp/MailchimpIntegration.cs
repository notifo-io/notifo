// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Mailchimp
{
    public sealed class MailchimpIntegration : IIntegration
    {
        private readonly IHttpClientFactory httpClientFactory;

        private static readonly IntegrationProperty ApiKeyProperty = new IntegrationProperty("apiKey", IntegrationPropertyType.Password)
        {
            EditorLabel = Texts.Mailchimp_ApiKeyLabel,
            EditorDescription = null,
            IsRequired = true
        };

        private static readonly IntegrationProperty FromEmailProperty = new IntegrationProperty("fromEmail", IntegrationPropertyType.Text)
        {
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
                "./integrations/mailchimp.png",
                new List<IntegrationProperty>
                {
                    ApiKeyProperty,
                    FromEmailProperty,
                    FromNameProperty
                },
                new HashSet<string>
                {
                    Providers.Email
                })
            {
                Description = Texts.Mailchimp_Description
            };

        public MailchimpIntegration(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public bool CanCreate(Type serviceType, ConfiguredIntegration configured)
        {
            return serviceType == typeof(IEmailSender);
        }

        public object? Create(Type serviceType, ConfiguredIntegration configured)
        {
            if (CanCreate(serviceType, configured))
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

                return new MailchimpEmailSender(httpClientFactory, apiKey, fromEmail, fromName);
            }

            return null;
        }
    }
}
