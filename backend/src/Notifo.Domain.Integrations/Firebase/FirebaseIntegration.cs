// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure.Validation;

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
            "<svg xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' style='fill:#fff;fill-rule:evenodd;stroke:#000;stroke-linecap:round;stroke-linejoin:round' viewBox='0 0 46.214 64.021'><use xlink:href='#a' x='1' y='1' width='100%' height='100%'/><symbol id='a' overflow='visible' style='overflow:visible' transform='translate(-1 -1)'><g style='fill-rule:nonzero;stroke:none'><path d='m30.346 23.115-6.406 5.96-5.944-11.99 3.076-6.896c.8-1.4 2.048-1.384 2.828 0z' style='fill:#ffa000'/><path d='m17.996 17.085 5.944 11.99L0 51.345Z' style='fill:#f57f17'/><path d='M37.352 14.006c1.144-1.1 2.328-.724 2.63.834l6.232 36.21-20.656 12.4c-.72.4-2.64.572-2.64.572s-1.748-.208-2.414-.6L0 51.346Z' style='fill:#ffca28'/><path d='M17.996 17.086.002 51.346l8.014-50.07C8.312-.284 9.2-.434 9.992.942Z' style='fill:#ffa000'/></g></symbol></svg>",
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

    public Task<IntegrationStatus> OnConfiguredAsync(IntegrationContext context, IntegrationConfiguration? previous,
        CancellationToken ct)
    {
        try
        {
            var firebaseProject = ProjectIdProperty.GetString(context.Properties);
            var firebaseCredentials = CredentialsProperty.GetString(context.Properties);

            messagingPool.GetMessaging(firebaseProject, firebaseCredentials);

            return Task.FromResult(IntegrationStatus.Verified);
        }
        catch (InvalidOperationException)
        {
            throw new ValidationException(Texts.Firebase_InvalidCredentials);
        }
    }
}
