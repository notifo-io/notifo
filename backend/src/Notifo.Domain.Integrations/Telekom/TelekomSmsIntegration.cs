﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Telekom;

public sealed partial class TelekomSmsIntegration(IHttpClientFactory httpClientFactory) : IIntegration
{
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

    public static readonly IntegrationProperty PhoneNumbersProperty = new IntegrationProperty("phoneNumbers", PropertyType.MultilineText)
    {
        EditorLabel = Texts.Telekom_PhoneNumbersLabel,
        EditorDescription = null,
        IsRequired = true,
        Summary = true
    };

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "Telekom",
            Texts.Telekom_Name,
            "<svg xmlns='http://www.w3.org/2000/svg' xml:space='preserve' width='86.205' height='56.127'><path d='M86.126 55.085c.047-.145.079-.298.079-.459V1.499c0-.162-.032-.316-.08-.462-.007-.02-.011-.04-.019-.06a1.492 1.492 0 0 0-.276-.46c-.008-.009-.009-.02-.017-.029-.005-.005-.011-.007-.016-.012a1.504 1.504 0 0 0-.442-.323c-.013-.006-.023-.014-.036-.02a1.48 1.48 0 0 0-.511-.123c-.018-.001-.035-.005-.053-.005C84.738.004 84.723 0 84.706 0H1.501c-.017 0-.033.004-.05.005L1.403.01a1.497 1.497 0 0 0-.518.125C.875.139.867.146.857.15.687.231.536.341.409.477.404.482.398.483.393.488.385.496.384.507.376.516a1.5 1.5 0 0 0-.277.461c-.008.02-.012.04-.019.061-.048.146-.08.3-.08.462v53.128c0 .164.033.32.082.468l.018.059a1.5 1.5 0 0 0 .28.462c.007.008.009.018.016.026.006.007.014.011.021.018.049.051.103.096.159.14.025.019.047.042.073.06.066.046.137.083.21.117.018.008.034.021.052.028.181.077.38.121.589.121h83.204c.209 0 .408-.043.589-.121.028-.012.054-.03.081-.044.062-.031.124-.063.181-.102.03-.021.057-.048.086-.071.051-.041.101-.082.145-.129l.025-.022c.008-.009.01-.021.018-.03a1.5 1.5 0 0 0 .275-.458c.01-.022.015-.043.022-.065zM3.001 4.901l25.247 23.061L3.001 51.207Zm51.746 21.931c-.104.068-.214.125-.301.221-.033.036-.044.083-.073.121l-11.27 10.294L5.367 2.999h75.472zm-24.275 3.161 11.619 10.613a1.496 1.496 0 0 0 2.023 0l11.475-10.481 25.243 23.002H5.345Zm27.342-1.9L83.205 4.901v46.33z'/></svg>",
            [
                ApiKeyProperty,
                PhoneNumberProperty,
                PhoneNumbersProperty
            ],
            [],
            new HashSet<string>
            {
                Providers.Messaging
            })
        {
            Description = Texts.Telekom_Description
        };
}
