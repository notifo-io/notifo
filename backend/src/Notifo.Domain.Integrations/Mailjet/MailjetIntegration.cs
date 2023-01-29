// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Mailjet;

public sealed partial class MailjetIntegration : IIntegration
{
    private readonly MailjetEmailServerPool serverPool;

    public static readonly IntegrationProperty ApiKeyProperty = new IntegrationProperty("apiKey", PropertyType.Password)
    {
        EditorLabel = Texts.Mailjet_ApiKeyLabel,
        EditorDescription = null,
        IsRequired = true
    };

    public static readonly IntegrationProperty ApiSecretProperty = new IntegrationProperty("apiSecret", PropertyType.Password)
    {
        EditorLabel = Texts.Mailjet_ApiSecretLabel,
        EditorDescription = null,
        IsRequired = true
    };

    public static readonly IntegrationProperty FromEmailProperty = new IntegrationProperty("fromEmail", PropertyType.Text)
    {
        Pattern = Patterns.Email,
        EditorLabel = Texts.Email_FromEmailLabel,
        EditorDescription = Texts.Email_FromEmailDescription,
        IsRequired = true,
        Summary = true
    };

    public static readonly IntegrationProperty FromNameProperty = new IntegrationProperty("fromName", PropertyType.Text)
    {
        EditorLabel = Texts.Email_FromNameLabel,
        EditorDescription = Texts.Email_FromNameDescription,
        IsRequired = true
    };

    public IntegrationDefinition Definition { get; }
        = new IntegrationDefinition(
            "Mailjet",
            Texts.Mailjet_Name,
            "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 520 520' style='enable-background:new 0 0 520 520' xml:space='preserve'><path d='M30.6 234.1 181.9 303l30.5-30.5-77.5-35.3 240.2-92.4L282.8 385l-35-77.1-30.5 30.5 1.6 3.5 67 147.5L445.4 74.6 30.6 234.1z' style='fill:#fead0d'/></svg>",
            new List<IntegrationProperty>
            {
                ApiKeyProperty,
                ApiSecretProperty,
                FromEmailProperty,
                FromNameProperty
            },
            new List<IntegrationProperty>(),
            new HashSet<string>
            {
                Providers.Email
            })
        {
            Description = Texts.Mailjet_Description
        };

    public MailjetIntegration(MailjetEmailServerPool serverPool)
    {
        this.serverPool = serverPool;
    }
}
