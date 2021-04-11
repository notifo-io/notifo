﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Smtp
{
    public sealed class SmtpIntegration : IIntegration
    {
        private readonly SmtpEmailServerPool serverPool;

        private static readonly IntegrationProperty HostProperty = new IntegrationProperty("host", IntegrationPropertyType.Text)
        {
            EditorLabel = Texts.SMTP_HostLabel,
            EditorDescription = null,
            IsRequired = true,
            Summary = true
        };

        private static readonly IntegrationProperty HostPortProperty = new IntegrationProperty("port", IntegrationPropertyType.Number)
        {
            EditorLabel = Texts.SMTP_PortLabel,
            EditorDescription = null,
            DefaultValue = "587"
        };

        private static readonly IntegrationProperty UsernameProperty = new IntegrationProperty("username", IntegrationPropertyType.Text)
        {
            EditorLabel = Texts.SMTP_UsernameLabel,
            EditorDescription = Texts.SMTP_UsernameHints,
            IsRequired = true
        };

        private static readonly IntegrationProperty PasswordProperty = new IntegrationProperty("password", IntegrationPropertyType.Password)
        {
            EditorLabel = Texts.SMTP_PasswordLabel,
            EditorDescription = Texts.SMTP_PasswordHints,
            IsRequired = true
        };

        private static readonly IntegrationProperty FromEmailProperty = new IntegrationProperty("fromEmail", IntegrationPropertyType.Text)
        {
            EditorLabel = Texts.SMTP_FromEmailLabel,
            EditorDescription = null,
            IsRequired = true
        };

        private static readonly IntegrationProperty FromNameProperty = new IntegrationProperty("fromName", IntegrationPropertyType.Text)
        {
            EditorLabel = Texts.SMTP_FromNameLabel,
            EditorDescription = null,
            IsRequired = true
        };

        public IntegrationDefinition Definition { get; }
            = new IntegrationDefinition(
                "SMTP",
                Texts.SMTP_Name,
                "./integrations/email.svg",
                new List<IntegrationProperty>
                {
                    HostProperty,
                    HostPortProperty,
                    UsernameProperty,
                    PasswordProperty,
                    FromEmailProperty,
                    FromNameProperty,
                },
                new HashSet<string>
                {
                    Providers.Email
                })
            {
                Description = Texts.SMTP_Description
            };

        public SmtpIntegration(IMemoryCache memoryCache)
        {
            serverPool = new SmtpEmailServerPool(memoryCache);
        }

        public bool CanCreate(Type serviceType, ConfiguredIntegration configured)
        {
            return serviceType == typeof(IEmailSender);
        }

        public object? Create(Type serviceType, ConfiguredIntegration configured)
        {
            if (CanCreate(serviceType, configured))
            {
                var host = HostProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(host))
                {
                    return null;
                }

                var port = HostProperty.GetInt(configured);

                if (port == 0)
                {
                    return null;
                }

                var username = UsernameProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(username))
                {
                    return null;
                }

                var password = PasswordProperty.GetString(configured);

                if (string.IsNullOrWhiteSpace(password))
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

                var options = new SmtpOptions
                {
                    Username = username,
                    Host = host,
                    HostPort = port,
                    Password = password
                };

                var server = serverPool.GetServer(options);

                return new SmtpEmailSender(server, fromEmail, fromName);
            }

            return null;
        }
    }
}
