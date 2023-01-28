// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Firebase;

public sealed partial class FirebaseIntegration : IIntegration
{
    private readonly FirebaseMessagingPool messagingPool;

    public static readonly IntegrationProperty ProjectIdProperty = new IntegrationProperty("projectId", PropertyType.Text)
    {
        EditorLabel = Texts.Firebase_ProjectIdLabel,
        EditorDescription = null,
        IsRequired = true,
        Summary = true
    };

    public static readonly IntegrationProperty CredentialsProperty = new IntegrationProperty("credentials", PropertyType.MultilineText)
    {
        EditorLabel = Texts.Firebase_CredentialsLabel,
        EditorDescription = Texts.Firebase_CredentialsHints,
        IsRequired = true
    };

    public static readonly IntegrationProperty SilentAndroidProperty = new IntegrationProperty("silentAndroid", PropertyType.Boolean)
    {
        EditorLabel = Texts.Firebase_SilentAndroidLabel,
        EditorDescription = Texts.Firebase_SilentAndroidDescription,
        IsRequired = true
    };

    public static readonly IntegrationProperty SilentISOProperty = new IntegrationProperty("silentIOS", PropertyType.Boolean)
    {
        EditorLabel = Texts.Firebase_SilentIOSLabel,
        EditorDescription = Texts.Firebase_SilentIOSDescription,
        IsRequired = true
    };

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "Firebase",
            Texts.Firebase_Name,
            "./integrations/firebase.svg",
            new List<IntegrationProperty>
            {
                ProjectIdProperty,
                SilentAndroidProperty,
                SilentISOProperty,
                CredentialsProperty
            },
            new List<IntegrationProperty>(),
            new HashSet<string>
            {
                Providers.MobilePush
            })
        {
            Description = Texts.Firebase_Description
        };

    public FirebaseIntegration(FirebaseMessagingPool messagingPool)
    {
        this.messagingPool = messagingPool;
    }
}
