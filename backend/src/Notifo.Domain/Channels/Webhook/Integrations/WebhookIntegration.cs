// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Webhook.Integrations
{
    public sealed class WebhookIntegration : IIntegration
    {
        private static readonly IntegrationProperty UrlProperty = new IntegrationProperty("Url", TntegrationPropertyType.Text)
        {
            IsRequired = true
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
                });

        public bool CanCreate<T>(ConfiguredIntegration configured)
        {
            return typeof(T) == typeof(WebhookDefinition);
        }

        public object? Create(Type implementationType, ConfiguredIntegration configured)
        {
            if (implementationType == typeof(WebhookDefinition))
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
