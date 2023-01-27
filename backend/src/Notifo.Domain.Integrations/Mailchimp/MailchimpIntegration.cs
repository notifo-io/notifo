// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Mailchimp;

public sealed class MailchimpIntegration : IIntegration
{
    private static readonly IntegrationProperty ApiKeyProperty = new IntegrationProperty("apiKey", PropertyType.Password)
    {
        EditorLabel = Texts.Mailchimp_ApiKeyLabel,
        EditorDescription = null,
        IsRequired = true
    };

    private static readonly IntegrationProperty FromEmailProperty = new IntegrationProperty("fromEmail", PropertyType.Text)
    {
        Pattern = Patterns.Email,
        EditorLabel = Texts.Email_FromEmailLabel,
        EditorDescription = Texts.Email_FromEmailDescription,
        IsRequired = true,
        Summary = true
    };

    private static readonly IntegrationProperty FromNameProperty = new IntegrationProperty("fromName", PropertyType.Text)
    {
        EditorLabel = Texts.Email_FromNameLabel,
        EditorDescription = Texts.Email_FromNameDescription,
        IsRequired = true
    };

    public IntegrationDefinition Definition { get; }
        = new IntegrationDefinition(
            "Mailchimp",
            Texts.Mailchimp_Name,
            "./integrations/mailchimp.svg",
            new List<IntegrationProperty>
            {
                ApiKeyProperty,
                FromEmailProperty,
                FromNameProperty
            },
            new List<UserProperty>(),
            new HashSet<string>
            {
                Providers.Email
            })
        {
            Description = Texts.Mailchimp_Description
        };

    public bool CanCreate(Type serviceType, IntegrationContext context)
    {
        return serviceType == typeof(IEmailSender);
    }

    public object? Create(Type serviceType, IntegrationContext context, IServiceProvider serviceProvider)
    {
        if (CanCreate(serviceType, context))
        {
            var apiKey = ApiKeyProperty.GetString(context.Properties);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return null;
            }

            var fromEmail = FromEmailProperty.GetString(context.Properties);

            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                return null;
            }

            var fromName = FromNameProperty.GetString(context.Properties);

            if (string.IsNullOrWhiteSpace(fromName))
            {
                return null;
            }

            return new MailchimpEmailSender(
                serviceProvider.GetRequiredService<IHttpClientFactory>(),
                apiKey,
                fromEmail,
                fromName);
        }

        return null;
    }
}
