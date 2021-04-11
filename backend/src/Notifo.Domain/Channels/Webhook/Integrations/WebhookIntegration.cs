// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Notifo.Domain.Integrations;
using Notifo.Domain.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Webhook.Integrations
{
    public sealed class WebhookIntegration : IIntegration
    {
        private static readonly IntegrationProperty UrlProperty = new IntegrationProperty("Url", IntegrationPropertyType.Text)
        {
            EditorLabel = Texts.Webhook_URLLabel,
            EditorDescription = Texts.Webhook_URLHints,
            IsRequired = true,
            Summary = true
        };

        public IntegrationDefinition Definition { get; } =
            new IntegrationDefinition(
                "Webhook",
                "Webhook",
                "./integrations/webhook.svg",
                new List<IntegrationProperty>
                {
                    UrlProperty
                },
                new HashSet<string>
                {
                    Providers.Webhook
                })
            {
                Description = Texts.Webhook_Description
            };

        public bool CanCreate(Type serviceType, ConfiguredIntegration configured)
        {
            return serviceType == typeof(WebhookDefinition);
        }

        public object? Create(Type serviceType, ConfiguredIntegration configured)
        {
            if (CanCreate(serviceType, configured))
            {
                var url = UrlProperty.GetString(configured);

                if (url == null)
                {
                    throw new DomainException("No URL configured for integration.");
                }

                return new WebhookDefinition
                {
                    Url = url
                };
            }

            return null;
        }
    }
}
