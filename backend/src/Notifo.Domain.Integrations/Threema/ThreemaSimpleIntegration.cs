﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.Threema;

public sealed partial class ThreemaSimpleIntegration(IHttpClientFactory httpClientFactory) : IIntegration
{
    public static readonly IntegrationProperty ApiIdentity = new IntegrationProperty("apiIdentity", PropertyType.Text)
    {
        EditorLabel = Texts.ThreemaSimple_ApiIdentityLabel,
        EditorDescription = null,
        IsRequired = true,
        Summary = true
    };

    public static readonly IntegrationProperty ApiSecret = new IntegrationProperty("apiSecret", PropertyType.Password)
    {
        EditorLabel = Texts.ThreemaSimple_ApiSecretLabel,
        EditorDescription = null,
        IsRequired = true
    };

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "ThreemaSimple",
            Texts.ThreemaSimple_Name,
            "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1024 1024'><path d='M1012.666 337.496c-10.628-47.808-29.912-88.146-61.468-128.58C883.33 121.954 766.456 57.982 631.874 34.13c-44.096-7.816-65.24-9.486-120.04-9.486s-75.944 1.67-120.04 9.486C257.212 57.98 140.338 121.952 72.47 208.914c-31.556 40.434-50.84 80.772-61.468 128.58C7.03 355.358 4.958 376.996 5 392.116c.036 12.846 2.03 36.756 6.002 54.622 14.414 64.838 44.812 116.854 98.822 169.096 26.718 25.844 26.756 25.912 25.212 45.558-1.516 19.272-18.19 54.262-58.55 122.862-9.478 16.112-17.234 30.332-17.234 31.602 0 2.922 2.29 2.896 16.712-.182 26.194-5.592 79.126-20.74 189.302-54.176 38.8-11.774 44.526-12.692 78.674-12.606 27.802.07 36.532.74 77.864 5.976 50.78 6.434 119.67 7.126 166.272 1.67 172.666-20.212 314.84-99.024 386.022-213.986 18.176-29.356 30.726-60.532 38.57-95.816 3.972-17.864 6.334-35.786 6.334-54.622-.002-22.106-2.364-36.754-6.336-54.618-10.628-47.808 3.972 17.864 0 0zM668.258 556.29c0 15.208-12.33 27.538-27.538 27.538H379.71c-15.208 0-27.538-12.33-27.538-27.538V399.622c0-15.208 12.33-27.538 27.538-27.538h5.602l.016-54.86c.008-30.174 1.158-60.612 2.556-67.644 3.872-19.482 18.868-41.54 38.26-56.274 19.402-14.742 38.892-22.762 65.286-26.864 61.252-9.52 125.512 24.302 141.614 74.532 3.206 10.002 4.07 25.27 4.07 71.904v59.206h3.604c15.208 0 27.538 12.33 27.538 27.538V556.29z' fill='#484848'/><path d='M498.992 215.8c-16.292 3.118-28.324 9.212-40.3 20.416-11.972 11.198-21.228 27.96-23.618 42.768-.862 5.344-1.572 28.476-1.578 51.408l-.01 41.692h155.436v-44.996c0-35.442-.534-47.044-2.512-54.646-9.94-38.174-49.608-63.876-87.418-56.642z' fill='#484848'/><path d='M845.374 926.498c0 40.239-32.621 72.86-72.86 72.86s-72.86-32.621-72.86-72.86 32.621-72.86 72.86-72.86 72.86 32.621 72.86 72.86zm-262.3 0c0 40.239-32.621 72.86-72.86 72.86s-72.86-32.621-72.86-72.86 32.621-72.86 72.86-72.86 72.86 32.621 72.86 72.86zm-262.298 0c0 40.239-32.621 72.86-72.86 72.86s-72.86-32.621-72.86-72.86 32.621-72.86 72.86-72.86 72.86 32.621 72.86 72.86z' fill='#3ae376'/></svg>",
            [
                ApiIdentity,
                ApiSecret
            ],
            [],
            new HashSet<string>
            {
                Providers.Messaging
            })
        {
            Description = Texts.ThreemaSimple_Description
        };
}
