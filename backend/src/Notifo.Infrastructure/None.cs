// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure;

public sealed class None
{
    public static readonly Type Type = typeof(None);

    public static readonly None Value = new None();

    private None()
    {
    }
}
