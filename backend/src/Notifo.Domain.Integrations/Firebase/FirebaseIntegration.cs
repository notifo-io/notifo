// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Firebase
{
    public sealed class FirebaseIntegration : IIntegration
    {
        private readonly FirebaseMobilePushSenderPool senderPool;

        private static readonly IntegrationProperty ProjectIdProperty = new IntegrationProperty("projectId", IntegrationPropertyType.Text)
        {
            EditorLabel = Texts.Firebase_ProjectIdLabel,
            EditorDescription = null,
            IsRequired = true,
            Summary = true
        };

        private static readonly IntegrationProperty CredentialsProperty = new IntegrationProperty("credentials", IntegrationPropertyType.MultilineText)
        {
            EditorLabel = Texts.Firebase_CredentialsLabel,
            EditorDescription = Texts.Firebase_CredentialsHints,
            IsRequired = true
        };

        public IntegrationDefinition Definition { get; }
            = new IntegrationDefinition(
                "Firebase",
                Texts.Firebase_Name,
                "./integrations/firebase.svg",
                new List<IntegrationProperty>
                {
                    ProjectIdProperty,
                    CredentialsProperty
                },
                new HashSet<string>
                {
                    Providers.MobilePush
                })
            {
                Description = Texts.Firebase_Description
            };

        public FirebaseIntegration(IMemoryCache memoryCache)
        {
            senderPool = new FirebaseMobilePushSenderPool(memoryCache);
        }

        public bool CanCreate(Type serviceType, ConfiguredIntegration configured)
        {
            return serviceType == typeof(IMobilePushSender);
        }

        public object? Create(Type serviceType, ConfiguredIntegration configured)
        {
            if (CanCreate(serviceType, configured))
            {
                var projectId = ProjectIdProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return null;
                }

                var credentials = CredentialsProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(credentials))
                {
                    return null;
                }

                return senderPool.GetSender(projectId, credentials);
            }

            return null;
        }
    }
}
