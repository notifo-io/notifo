﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.Resources;
using Notifo.Domain.Users;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class MessageBirdIntegration : IIntegration
    {
        private readonly MessageBirdClientPool clientPool;

        private static readonly IntegrationProperty SendSmsProperty = new IntegrationProperty("sendSms", PropertyType.Boolean)
        {
            EditorLabel = Texts.MessageBird_SendSmsLabel,
            EditorDescription = null,
            DefaultValue = "true"
        };

        private static readonly IntegrationProperty SendWhatsAppProperty = new IntegrationProperty("sendWhatsApp", PropertyType.Boolean)
        {
            EditorLabel = Texts.MessageBird_SendWhatsAppLabel,
            EditorDescription = null,
            DefaultValue = "true"
        };

        private static readonly IntegrationProperty AccessKeyProperty = new IntegrationProperty("accessKey", PropertyType.Text)
        {
            EditorLabel = Texts.MessageBird_AccessKeyLabel,
            EditorDescription = null,
            IsRequired = true
        };

        private static readonly IntegrationProperty PhoneNumberProperty = new IntegrationProperty("phoneNumber", PropertyType.Number)
        {
            EditorLabel = Texts.MessageBird_PhoneNumberLabel,
            EditorDescription = null,
            IsRequired = true,
            Summary = true
        };

        private static readonly IntegrationProperty PhoneNumbersProperty = new IntegrationProperty("phoneNumbers", PropertyType.MultilineText)
        {
            EditorLabel = Texts.MessageBird_PhoneNumbersLabel,
            EditorDescription = Texts.MessageBird_PhoneNumbersDescription,
            IsRequired = false,
            Summary = false
        };

        private static readonly IntegrationProperty WhatsAppChannelIdProperty = new IntegrationProperty("whatsAppChannelId", PropertyType.Text)
        {
            EditorLabel = Texts.MessageBird_WhatsAppChannelIdLabel,
            EditorDescription = Texts.MessageBird_WhatsAppChannelIdDescription,
            IsRequired = false,
            Summary = false
        };

        private static readonly IntegrationProperty WhatsAppTemplateNameProperty = new IntegrationProperty("whatsAppTemplateName", PropertyType.Text)
        {
            EditorLabel = Texts.MessageBird_WhatsAppTemplateNameLabel,
            EditorDescription = Texts.MessageBird_WhatsAppTemplateNameDescription,
            IsRequired = false,
            Summary = false
        };

        private static readonly IntegrationProperty WhatsAppTemplateNamespaceProperty = new IntegrationProperty("whatsAppTemplateNamespace", PropertyType.Text)
        {
            EditorLabel = Texts.MessageBird_WhatsAppTemplateNamespaceLabel,
            EditorDescription = Texts.MessageBird_WhatsAppTemplateNamespaceDescription,
            IsRequired = false,
            Summary = false
        };

        public IntegrationDefinition Definition { get; } =
            new IntegrationDefinition(
                "MessageBird",
                Texts.MessageBird_Name,
                "./integrations/messagebird.svg",
                new List<IntegrationProperty>
                {
                    SendSmsProperty,
                    SendWhatsAppProperty,
                    AccessKeyProperty,
                    PhoneNumberProperty,
                    PhoneNumbersProperty,
                    WhatsAppChannelIdProperty,
                    WhatsAppTemplateNamespaceProperty,
                    WhatsAppTemplateNameProperty
                },
                new List<UserProperty>(),
                new HashSet<string>
                {
                    Providers.Sms,
                    Providers.Messaging
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
            return serviceType == typeof(ISmsSender) || serviceType == typeof(IMessagingSender);
        }

        public object? Create(Type serviceType, string id, ConfiguredIntegration configured, IServiceProvider serviceProvider)
        {
            var sms = CreateSms(serviceType, id, configured, serviceProvider);

            if (sms != null)
            {
                return sms;
            }

            var messaging = CreateMessaging(serviceType, id, configured, serviceProvider);

            if (messaging != null)
            {
                return messaging;
            }

            return null;
        }

        private ISmsSender? CreateSms(Type serviceType, string id, ConfiguredIntegration configured, IServiceProvider serviceProvider)
        {
            if (serviceType != typeof(ISmsSender) || !SendSmsProperty.GetBoolean(configured))
            {
                return null;
            }

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

            var phoneNumbersString = PhoneNumbersProperty.GetString(configured);
            var phoneNumbersMap = ParsePhoneNumbers(phoneNumbersString);

            var client = clientPool.GetClient(accessKey);

            return new MessageBirdSmsSender(
                client,
                serviceProvider.GetRequiredService<ISmsCallback>(),
                serviceProvider.GetRequiredService<ISmsUrl>(),
                id,
                phoneNumber.ToString(CultureInfo.InvariantCulture),
                phoneNumbersMap);
        }

        private IMessagingSender? CreateMessaging(Type serviceType, string id, ConfiguredIntegration configured, IServiceProvider serviceProvider)
        {
            if (serviceType != typeof(IMessagingSender) || !SendWhatsAppProperty.GetBoolean(configured))
            {
                return null;
            }

            var accessKey = AccessKeyProperty.GetString(configured);

            if (string.IsNullOrWhiteSpace(accessKey))
            {
                return null;
            }

            var channelId = WhatsAppChannelIdProperty.GetString(configured);

            if (string.IsNullOrWhiteSpace(channelId))
            {
                return null;
            }

            var templateNamespace = WhatsAppTemplateNamespaceProperty.GetString(configured);

            if (string.IsNullOrWhiteSpace(templateNamespace))
            {
                return null;
            }

            var templateName = WhatsAppTemplateNameProperty.GetString(configured);

            if (string.IsNullOrWhiteSpace(templateName))
            {
                return null;
            }

            var client = clientPool.GetClient(accessKey);

            return new MessageBirdWhatsAppSender(
                client,
                serviceProvider.GetRequiredService<IMessagingCallback>(),
                serviceProvider.GetRequiredService<IMessagingUrl>(),
                serviceProvider.GetRequiredService<IUserStore>(),
                id,
                channelId,
                templateNamespace,
                templateName);
        }

        private static Dictionary<string, string>? ParsePhoneNumbers(string? source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            var result = new Dictionary<string, string>();

            foreach (var line in source.Split('\n'))
            {
                if (line.Length > 5)
                {
                    var parts = line.Split(':');

                    if (parts.Length == 2)
                    {
                        var countryCode = parts[0].Trim();

                        result[countryCode] = parts[1].Trim();
                    }
                    else
                    {
                        var countryCode = line[..2].Trim();

                        result[countryCode] = line;
                    }
                }
            }

            return result;
        }
    }
}
