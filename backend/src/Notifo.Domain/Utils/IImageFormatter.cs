// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Utils
{
    public interface IImageFormatter
    {
        string Format(string? url, string preset);

        string? FormatWhenSet(string? url, string preset);
    }
}
