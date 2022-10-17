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
        string? AddProxy(string? url);

        string? AddPreset(string? url, string? preset);
    }
}
