// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Texts
{
    public sealed class LocalizedText : Dictionary<string, string>, IEquatable<LocalizedText>
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

        public LocalizedText Trim()
        {
            if (Values.All(x => x.Trim() == x))
            {
                return this;
            }

            var result = new LocalizedText();

            foreach (var (key, value) in this)
            {
                result[key] = value.Trim();
            }

            return result;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as LocalizedText);
        }

        public bool Equals(LocalizedText? other)
        {
            return this.EqualsDictionary(other);
        }

        public override int GetHashCode()
        {
            return this.DictionaryHashCode();
        }
    }
}
