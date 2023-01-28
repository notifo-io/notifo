// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Smtp;

public sealed partial class SmtpIntegration : IIntegration
{
    private readonly SmtpEmailServerPool serverPool;

    public static readonly IntegrationProperty HostProperty = new IntegrationProperty("host", PropertyType.Text)
    {
        EditorLabel = Texts.SMTP_HostLabel,
        EditorDescription = null,
        IsRequired = true,
        Summary = true
    };

    public static readonly IntegrationProperty HostPortProperty = new IntegrationProperty("port", PropertyType.Number)
    {
        EditorLabel = Texts.SMTP_PortLabel,
        EditorDescription = null,
        DefaultValue = "587"
    };

    public static readonly IntegrationProperty UsernameProperty = new IntegrationProperty("username", PropertyType.Text)
    {
        EditorLabel = Texts.SMTP_UsernameLabel,
        EditorDescription = Texts.SMTP_UsernameHints
    };

    public static readonly IntegrationProperty PasswordProperty = new IntegrationProperty("password", PropertyType.Password)
    {
        EditorLabel = Texts.SMTP_PasswordLabel,
        EditorDescription = Texts.SMTP_PasswordHints
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

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
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
                FromNameProperty
            },
            new List<IntegrationProperty>(),
            new HashSet<string>
            {
                Providers.Email
            })
        {
            Description = Texts.SMTP_Description
        };

    public SmtpIntegration(SmtpEmailServerPool serverPool)
    {
        this.serverPool = serverPool;
    }
}
