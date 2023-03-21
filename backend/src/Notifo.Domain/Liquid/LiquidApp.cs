// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Infrastructure;

namespace Notifo.Domain.Liquid;

public sealed class LiquidApp
{
    private readonly App app;

    public string? Name => app.Name.OrNull();

    public LiquidApp(App app)
    {
        this.app = app;
    }
}
