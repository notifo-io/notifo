// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Mailchimp;

public sealed partial class MailchimpIntegration : IIntegration
{
    private readonly IHttpClientFactory httpClientFactory;

    public static readonly IntegrationProperty ApiKeyProperty = new IntegrationProperty("apiKey", PropertyType.Password)
    {
        EditorLabel = Texts.Mailchimp_ApiKeyLabel,
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
            "Mailchimp",
            Texts.Mailchimp_Name,
            "./integrations/mailchimp.svg",
            new List<IntegrationProperty>
            {
                ApiKeyProperty,
                FromEmailProperty,
                FromNameProperty
            },
            new List<IntegrationProperty>(),
            new HashSet<string>
            {
                Providers.Email
            })
        {
            Description = Texts.Mailchimp_Description
        };

    public MailchimpIntegration(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }
}
