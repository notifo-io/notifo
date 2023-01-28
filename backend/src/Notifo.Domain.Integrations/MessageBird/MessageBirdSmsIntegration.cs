// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.MessageBird;

public sealed partial class MessageBirdSmsIntegration : IIntegration
{
    private readonly ISmsCallback callback;
    private readonly MessageBirdClientPool clientPool;

    public static readonly IntegrationProperty AccessKeyProperty = new IntegrationProperty("accessKey", PropertyType.Text)
    {
        EditorLabel = Texts.MessageBird_AccessKeyLabel,
        EditorDescription = null,
        IsRequired = true
    };

    public static readonly IntegrationProperty OriginatorProperty = new IntegrationProperty("phoneNumber", PropertyType.Number)
    {
        EditorLabel = Texts.MessageBird_OriginatorNameLabel,
        EditorDescription = Texts.MessageBird_OriginatorNameDescription,
        IsRequired = false,
        Summary = true
    };

    public static readonly IntegrationProperty PhoneNumberProperty = new IntegrationProperty("phoneNumber", PropertyType.Number)
    {
        EditorLabel = Texts.MessageBird_PhoneNumberLabel,
        EditorDescription = null,
        IsRequired = false,
        Summary = true
    };

    public static readonly IntegrationProperty PhoneNumbersProperty = new IntegrationProperty("phoneNumbers", PropertyType.MultilineText)
    {
        EditorLabel = Texts.MessageBird_PhoneNumbersLabel,
        EditorDescription = Texts.MessageBird_PhoneNumbersDescription,
        IsRequired = false,
        Summary = false
    };

    public static readonly IntegrationProperty WhatsAppChannelIdProperty = new IntegrationProperty("whatsAppChannelId", PropertyType.Text)
    {
        EditorLabel = Texts.MessageBird_WhatsAppChannelIdLabel,
        EditorDescription = Texts.MessageBird_WhatsAppChannelIdDescription,
        IsRequired = false,
        Summary = false
    };

    public static readonly IntegrationProperty WhatsAppTemplateNameProperty = new IntegrationProperty("whatsAppTemplateName", PropertyType.Text)
    {
        EditorLabel = Texts.MessageBird_WhatsAppTemplateNameLabel,
        EditorDescription = Texts.MessageBird_WhatsAppTemplateNameDescription,
        IsRequired = false,
        Summary = false
    };

    public static readonly IntegrationProperty WhatsAppTemplateNamespaceProperty = new IntegrationProperty("whatsAppTemplateNamespace", PropertyType.Text)
    {
        EditorLabel = Texts.MessageBird_WhatsAppTemplateNamespaceLabel,
        EditorDescription = Texts.MessageBird_WhatsAppTemplateNamespaceDescription,
        IsRequired = false,
        Summary = false
    };

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "MessageBird",
            Texts.MessageBird_Name,
            "./integrations/messagebird.svg",
            new List<IntegrationProperty>
            {
                AccessKeyProperty,
                PhoneNumberProperty,
                PhoneNumbersProperty,
                WhatsAppChannelIdProperty,
                WhatsAppTemplateNamespaceProperty,
                WhatsAppTemplateNameProperty
            },
            new List<IntegrationProperty>(),
            new HashSet<string>
            {
                Providers.Sms
            })
        {
            Description = Texts.MessageBird_Description
        };

    public MessageBirdSmsIntegration(MessageBirdClientPool clientPool, ISmsCallback callback)
    {
        this.clientPool = clientPool;
        this.callback = callback;
    }
}
