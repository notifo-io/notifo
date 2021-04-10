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

namespace Notifo.Domain.Integrations.Firebase
{
    public sealed class FirebaseIntegration : IIntegration
    {
        private readonly FirebaseMobilePushSenderPool senderPool = new FirebaseMobilePushSenderPool();

        private static readonly IntegrationProperty ProjectIdProperty = new IntegrationProperty("projectId", TntegrationPropertyType.Text)
        {
            IsRequired = true
        };

        private static readonly IntegrationProperty CredentialsProperty = new IntegrationProperty("credentials", TntegrationPropertyType.Text)
        {
            IsRequired = true
        };

        public IntegrationDefinition Definition { get; }
            = new IntegrationDefinition(
                "Firebase",
                "Firebase",
                "./integrations/firebase.svg",
                new List<IntegrationProperty>
                {
                    ProjectIdProperty,
                    CredentialsProperty
                },
                new HashSet<string>
                {
                    Providers.MobilePush
                });

        public bool CanCreate<T>(ConfiguredIntegration configured)
        {
            return typeof(T) == typeof(IMobilePushSender);
        }

        public object? Create(Type implementationType, ConfiguredIntegration configured)
        {
            if (implementationType == typeof(IMobilePushSender))
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
