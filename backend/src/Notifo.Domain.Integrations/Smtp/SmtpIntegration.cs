// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Smtp
{
    public sealed class SmtpIntegration : IIntegration
    {
        private readonly SmtpEmailServerPool smtpEmailServerPool = new SmtpEmailServerPool();

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

        private static readonly IntegrationProperty HostProperty = new IntegrationProperty("host", IntegrationPropertyType.Text)
        {
            IsRequired = true, Summary = true
        };

        private static readonly IntegrationProperty UsernameProperty = new IntegrationProperty("username", IntegrationPropertyType.Text)
        {
            IsRequired = true
        };

        private static readonly IntegrationProperty PasswordProperty = new IntegrationProperty("password", IntegrationPropertyType.Text)
        {
            IsRequired = true
        };

        private static readonly IntegrationProperty HostPortProperty = new IntegrationProperty("port", IntegrationPropertyType.Number)
        {
            DefaultValue = "587"
        };

        public IntegrationDefinition Definition { get; }
            = new IntegrationDefinition(
                "SMTP",
                Texts.SMTP_Name,
                "./integrations/email.svg",
                new List<IntegrationProperty>
                {
                    UsernameProperty,
                    FromEmailProperty,
                    FromNameProperty,
                    HostProperty,
                    HostPortProperty,
                    PasswordProperty,
                },
                new HashSet<string>
                {
                    Providers.Email
                })
            {
                Description = Texts.SMTP_Description
            };

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

                var server = smtpEmailServerPool.GetServer(options);

                return new SmtpEmailSender(server, fromEmail, fromName);
            }

            return null;
        }
    }
}
