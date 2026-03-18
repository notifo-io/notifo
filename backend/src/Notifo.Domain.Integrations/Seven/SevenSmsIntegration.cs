// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Seven;

public sealed partial class SevenSmsIntegration(SevenSmsClientPool clientPool) : IIntegration
{
    public static readonly IntegrationProperty ApiKeyProperty = new IntegrationProperty("apiKey", PropertyType.Password)
    {
        EditorLabel = Texts.Seven_ApiKeyLabel,
        EditorDescription = null,
        IsRequired = true
    };

    public static readonly IntegrationProperty FromProperty = new IntegrationProperty("from", PropertyType.Text)
    {
        EditorLabel = Texts.Seven_FromLabel,
        EditorDescription = Texts.Seven_FromDescription,
        IsRequired = false,
        Summary = true
    };

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "Seven",
            Texts.Seven_Name,
            "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'><rect width='200' height='200' rx='24' fill='#59C864'/><path d='M45 60h110l-65 105h-20l50-85H45z' fill='#fff'/></svg>",
            [
                ApiKeyProperty,
                FromProperty
            ],
            [],
            new HashSet<string>
            {
                Providers.Sms
            })
        {
            Description = Texts.Seven_Description
        };
}
