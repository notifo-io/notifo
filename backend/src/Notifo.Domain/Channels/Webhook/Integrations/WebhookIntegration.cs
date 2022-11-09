// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.Resources;

namespace Notifo.Domain.Channels.Webhook.Integrations;

public sealed class WebhookIntegration : IIntegration
{
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

    private static readonly IntegrationProperty NameProperty = new IntegrationProperty("Name", PropertyType.Text)
    {
        EditorLabel = Texts.Webhook_NameLabel,
        EditorDescription = Texts.Webhook_NameHints,
        IsRequired = false,
        Summary = true
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
                NameProperty,
                HttpUrlProperty,
                HttpMethodProperty,
                SendConfirmProperty
            },
            new List<UserProperty>(),
            new HashSet<string>
            {
                Providers.Webhook
            })
        {
            Description = Texts.Webhook_Description
        };

    public bool CanCreate(Type serviceType, string id, ConfiguredIntegration configured)
    {
        return serviceType == typeof(WebhookDefinition);
    }

    public object? Create(Type serviceType, string id, ConfiguredIntegration configured, IServiceProvider serviceProvider)
    {
        if (CanCreate(serviceType, id, configured))
        {
            var url = HttpUrlProperty.GetString(configured);

            if (url == null)
            {
                return null;
            }

            var httpMethod = HttpMethodProperty.GetString(configured);

            return new WebhookDefinition
            {
                Name = NameProperty.GetString(configured),
                HttpUrl = url,
                HttpMethod = httpMethod ?? "POST",
                SendAlways = SendAlwaysProperty.GetBoolean(configured),
                SendConfirm = SendConfirmProperty.GetBoolean(configured)
            };
        }

        return null;
    }
}
