// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class MessageBirdIntegration : IIntegration
    {
        private readonly MessageBirdSmsSenderPool senderPool;

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

        public MessageBirdIntegration(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            senderPool = new MessageBirdSmsSenderPool(httpClientFactory, memoryCache);
        }

        public bool CanCreate(Type serviceType, ConfiguredIntegration configured)
        {
            return serviceType == typeof(ISmsSender);
        }

        public object? Create(Type serviceType, ConfiguredIntegration configured)
        {
            if (CanCreate(serviceType, configured))
            {
                var accessKey = AccessKeyProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(accessKey))
                {
                    return null;
                }

                var phoneNumber = PhoneNumberProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    return null;
                }

                return senderPool.GetServer(accessKey, phoneNumber);
            }

            return null;
        }
    }
}
