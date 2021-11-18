// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Texts
{
    public sealed class LocalizedText : Dictionary<string, string>
    {
        public LocalizedText()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public LocalizedText(IReadOnlyDictionary<string, string> source)
            : base(source, StringComparer.OrdinalIgnoreCase)
        {
        }

        public LocalizedText Clone()
        {
            return new LocalizedText(this);
        }
    }
}
