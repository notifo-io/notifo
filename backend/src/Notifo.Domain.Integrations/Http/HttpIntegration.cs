// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Http;

public sealed partial class HttpIntegration : IIntegration
{
    private readonly IHttpClientFactory httpClientFactory;

    private static readonly IntegrationProperty HttpUrlProperty = new IntegrationProperty("Url", PropertyType.Text)
    {
        EditorLabel = Texts.Webhook_URLLabel,
        EditorDescription = Texts.Webhook_URLHints,
        IsRequired = true,
        Summary = true
    };

    private static readonly IntegrationProperty HttpMethodProperty = new IntegrationProperty("Method", PropertyType.Text)
    {
        EditorLabel = Texts.Webhook_MethodLabel,
        EditorDescription = Texts.Webhook_MethodHints,
        AllowedValues = new[] { "GET", "PATCH", "POST", "PUT", "DELETE" },
        IsRequired = false,
        Summary = false
    };

    private static readonly IntegrationProperty SendAlwaysProperty = new IntegrationProperty("SendAlways", PropertyType.Boolean)
    {
        EditorLabel = Texts.Webhook_SendAlwaysLabel,
        EditorDescription = Texts.Webhook_SendAlwaysHints,
        IsRequired = false,
        Summary = false
    };

    private static readonly IntegrationProperty SendConfirmProperty = new IntegrationProperty("SendConfirm", PropertyType.Boolean)
    {
        EditorLabel = Texts.Webhook_SendConfirmLabel,
        EditorDescription = Texts.Webhook_SendConfirmHints,
        IsRequired = false,
        Summary = false
    };

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "Webhook",
            "Webhook",
            "./integrations/webhook.svg",
            new List<IntegrationProperty>
            {
                HttpUrlProperty,
                HttpMethodProperty,
                SendConfirmProperty
            },
            new List<IntegrationProperty>(),
            new HashSet<string>
            {
                    Providers.Webhook
            })
        {
            Description = Texts.Webhook_Description
        };

    public HttpIntegration(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }
}
