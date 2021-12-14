// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Integrations
{
    public sealed record UserProperty(string Name)
    {
        public string? EditorDescription { get; init; }

        public string? EditorLabel { get; init; }
    }
}
