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
            "./integrations/mailjet.svg",
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
