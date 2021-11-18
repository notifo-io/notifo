﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Threema
{
    public sealed class ThreemaSimpleIntegration : IIntegration
    {
        private static readonly IntegrationProperty ApiIdentity = new IntegrationProperty("apiIdentity", IntegrationPropertyType.Text)
        {
            EditorLabel = Texts.ThreemaSimple_ApiIdentityLabel,
            EditorDescription = null,
            IsRequired = true,
            Summary = true
        };

        private static readonly IntegrationProperty ApiSecret = new IntegrationProperty("apiSecret", IntegrationPropertyType.Password)
        {
            EditorLabel = Texts.ThreemaSimple_ApiSecretLabel,
            EditorDescription = null,
            IsRequired = true
        };

        public IntegrationDefinition Definition { get; } =
            new IntegrationDefinition(
                "ThreemaSimple",
                Texts.ThreemaSimple_Name,
                "./integrations/threema.svg",
                new List<IntegrationProperty>
                {
                    ApiIdentity,
                    ApiSecret
                },
                new HashSet<string>
                {
                    Providers.Messaging
                })
            {
                Description = Texts.ThreemaSimple_Description
            };

        public bool CanCreate(Type serviceType, string id, ConfiguredIntegration configured)
        {
            return serviceType == typeof(IMessagingSender);
        }

        public object? Create(Type serviceType, string id, ConfiguredIntegration configured, IServiceProvider serviceProvider)
        {
            if (CanCreate(serviceType, id, configured))
            {
                var apiIdentity = ApiIdentity.GetString(configured);

                if (string.IsNullOrWhiteSpace(apiIdentity))
                {
                    return null;
                }

                var apiSecret = ApiSecret.GetString(configured);

                if (string.IsNullOrWhiteSpace(apiSecret))
                {
                    return null;
                }

                return new ThreemaSimpleMessagingSender(
                    serviceProvider.GetRequiredService<IHttpClientFactory>(),
                    apiIdentity,
                    apiSecret);
            }

            return null;
        }
    }
}
