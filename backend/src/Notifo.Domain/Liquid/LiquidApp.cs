// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Infrastructure;

namespace Notifo.Domain.Liquid;

public sealed class LiquidApp(App app)
{
    private readonly App app = app;

    public string? Name => app.Name.OrNull();

    public static void Describe(LiquidProperties properties)
    {
        properties.AddString("name",
            "The name of the app.");
    }
}
