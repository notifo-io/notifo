// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Telekom;

public sealed partial class TelekomSmsIntegration : IIntegration
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ISmsCallback callback;

    public static readonly IntegrationProperty ApiKeyProperty = new IntegrationProperty("apiKey", PropertyType.Text)
    {
        EditorLabel = Texts.Telekom_ApiKeyLabel,
        EditorDescription = null,
        IsRequired = true
    };

    public static readonly IntegrationProperty PhoneNumberProperty = new IntegrationProperty("phoneNumber", PropertyType.Number)
    {
        EditorLabel = Texts.Telekom_PhoneNumberLabel,
        EditorDescription = null,
        IsRequired = true,
        Summary = true
    };

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "Telekom",
            Texts.Telekom_Name,
            "./integrations/telekom.svg",
            new List<IntegrationProperty>
            {
                ApiKeyProperty,
                PhoneNumberProperty
            },
            new List<IntegrationProperty>(),
            new HashSet<string>
            {
                Providers.Messaging
            })
        {
            Description = Texts.Telekom_Description
        };

    public TelekomSmsIntegration(IHttpClientFactory httpClientFactory, ISmsCallback callback)
    {
        this.httpClientFactory = httpClientFactory;

        this.callback = callback;
    }
}
