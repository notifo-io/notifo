// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Utils;

namespace Notifo.Domain.Channels.Email
{
    internal sealed class FakeImageFormatter : IImageFormatter
    {
        public string Format(string? url, string preset)
        {
            return url ?? string.Empty;
        }

        public string? FormatWhenSet(string? url, string preset)
        {
            return url;
        }
    }
}
