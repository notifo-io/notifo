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

namespace Notifo.Domain.Integrations.Smtp
{
    public sealed class SmtpIntegration : IIntegration
    {
        private readonly SmtpEmailServerPool smtpEmailServerPool = new SmtpEmailServerPool();

        private static readonly IntegrationProperty HostProperty = new IntegrationProperty("host", TntegrationPropertyType.Text)
        {
            IsRequired = true
        };

        private static readonly IntegrationProperty UsernameProperty = new IntegrationProperty("username", TntegrationPropertyType.Text)
        {
            IsRequired = true
        };

        private static readonly IntegrationProperty PasswordProperty = new IntegrationProperty("password", TntegrationPropertyType.Text)
        {
            IsRequired = true
        };

        private static readonly IntegrationProperty HostPortProperty = new IntegrationProperty("port", TntegrationPropertyType.Number)
        {
            DefaultValue = 587
        };

        private static readonly IntegrationProperty FromEmailProperty = new IntegrationProperty("fromEmail", TntegrationPropertyType.Text)
        {
            IsRequired = true
        };

        private static readonly IntegrationProperty FromNameProperty = new IntegrationProperty("fromName", TntegrationPropertyType.Text)
        {
            IsRequired = true
        };

        public IntegrationDefinition Definition { get; }
            = new IntegrationDefinition(
                "SMTP",
                "SMTP Server",
                "./integrations/smtp.svg",
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
                });

        public bool CanCreate<T>(ConfiguredIntegration configured)
        {
            return typeof(T) == typeof(IEmailSender);
        }

        public object? Create(Type implementationType, ConfiguredIntegration configured)
        {
            if (implementationType == typeof(IEmailSender))
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
