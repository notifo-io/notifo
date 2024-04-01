// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Liquid;

public sealed class LiquidPropertiesProvider
{
    public LiquidProperties GetProperties()
    {
        var properties = new LiquidProperties();

        properties.AddObject("app", () =>
        {
            LiquidApp.Describe(properties);
        }, "The current app.");

        properties.AddObject("user", () =>
        {
            LiquidUser.Describe(properties);
        }, "The current user.");

        properties.AddObject("notification", () =>
        {
            LiquidNotification.Describe(properties);
        }, "The first and usually single notifications. For emails multiple notifications can be grouped in one template.");

        properties.AddArray("notifications",
            "The list of notifications. Usually it is only one, but for emails multiple notifications can be grouped in one template.");

        return properties;
    }
}
