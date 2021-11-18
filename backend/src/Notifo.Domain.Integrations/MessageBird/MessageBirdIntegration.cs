// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class MessageBirdIntegration : IIntegration
    {
        private readonly MessageBirdClientPool clientPool;

        private static readonly IntegrationProperty AccessKeyProperty = new IntegrationProperty("accessKey", IntegrationPropertyType.Text)
        {
            EditorLabel = Texts.MessageBird_AccessKeyLabel,
            EditorDescription = null,
            IsRequired = true
        };

        private static readonly IntegrationProperty PhoneNumberProperty = new IntegrationProperty("phoneNumber", IntegrationPropertyType.Number)
        {
            EditorLabel = Texts.MessageBird_PhoneNumberLabel,
            EditorDescription = null,
            IsRequired = true,
            Summary = true
        };

        public IntegrationDefinition Definition { get; } =
            new IntegrationDefinition(
                "MessageBird",
                Texts.MessageBird_Name,
                "./integrations/messagebird.svg",
                new List<IntegrationProperty>
                {
                    AccessKeyProperty,
                    PhoneNumberProperty
                },
                new HashSet<string>
                {
                    Providers.Sms
                })
            {
                Description = Texts.MessageBird_Description
            };

        public MessageBirdIntegration(MessageBirdClientPool clientPool)
        {
            this.clientPool = clientPool;
        }

        public bool CanCreate(Type serviceType, string id, ConfiguredIntegration configured)
        {
            return serviceType == typeof(ISmsSender);
        }

        public object? Create(Type serviceType, string id, ConfiguredIntegration configured, IServiceProvider serviceProvider)
        {
            if (CanCreate(serviceType, id, configured))
            {
                var accessKey = AccessKeyProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(accessKey))
                {
                    return null;
                }

                var phoneNumber = PhoneNumberProperty.GetNumber(configured);

                if (phoneNumber == 0)
                {
                    return null;
                }

                var client = clientPool.GetServer(accessKey, phoneNumber);

                return new MessageBirdSmsSender(
                    client,
                    serviceProvider.GetRequiredService<ISmsCallback>(),
                    serviceProvider.GetRequiredService<ISmsUrl>(),
                    id);
            }

            return null;
        }
    }
}
