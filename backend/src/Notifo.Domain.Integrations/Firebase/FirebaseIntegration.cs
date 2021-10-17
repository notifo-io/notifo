// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.MobilePush;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Firebase
{
    public sealed class FirebaseIntegration : IIntegration
    {
        private readonly FirebaseMessagingPool messagingPool;

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

        private static readonly IntegrationProperty SilentAndroidProperty = new IntegrationProperty("silentAndroid", IntegrationPropertyType.Boolean)
        {
            EditorLabel = Texts.Firebase_SilentAndroidLabel,
            EditorDescription = Texts.Firebase_SilentAndroidDescription,
            IsRequired = true
        };

        private static readonly IntegrationProperty SilentISOProperty = new IntegrationProperty("silentIOS", IntegrationPropertyType.Boolean)
        {
            EditorLabel = Texts.Firebase_SilentIOSLabel,
            EditorDescription = Texts.Firebase_SilentIOSDescription,
            IsRequired = true
        };

        public IntegrationDefinition Definition { get; } =
            new IntegrationDefinition(
                "Firebase",
                Texts.Firebase_Name,
                "./integrations/firebase.svg",
                new List<IntegrationProperty>
                {
                    ProjectIdProperty,
                    SilentAndroidProperty,
                    SilentISOProperty,
                    CredentialsProperty
                },
                new HashSet<string>
                {
                    Providers.MobilePush
                })
            {
                Description = Texts.Firebase_Description
            };

        public FirebaseIntegration(FirebaseMessagingPool messagingPool)
        {
            this.messagingPool = messagingPool;
        }

        public bool CanCreate(Type serviceType, string id, ConfiguredIntegration configured)
        {
            return serviceType == typeof(IMobilePushSender);
        }

        public object? Create(Type serviceType, string id, ConfiguredIntegration configured)
        {
            if (CanCreate(serviceType, id, configured))
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

                var sendSilentIOS = SilentISOProperty.GetBoolean(configured);
                var sendSilentAndroid = SilentAndroidProperty.GetBoolean(configured);

                return new FirebaseMobilePushSender(() => messagingPool.GetMessaging(projectId, credentials),
                    sendSilentIOS,
                    sendSilentAndroid);
            }

            return null;
        }
    }
}
