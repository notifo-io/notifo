// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;

namespace Notifo.Infrastructure.Texts;

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

    public string SelectTextByCulture(bool first = false)
    {
        return SelectTextByCulture(CultureInfo.CurrentCulture, first);
    }

    public string SelectTextByCulture(CultureInfo cultureInfo, bool first = false)
    {
        return SelectText(cultureInfo.ToString(), cultureInfo.TwoLetterISOLanguageName, first);
    }

    public string SelectText(string language, bool first = false)
    {
        if (TryGetValue(language, out var text))
        {
            return text;
        }

        if (first && Count > 0)
        {
            return Values.First();
        }

        return string.Empty;
    }

    public string SelectText(string language1, string language2, bool first = false)
    {
        if (TryGetValue(language1, out var text))
        {
            return text;
        }

        if (TryGetValue(language2, out text))
        {
            return text;
        }

        if (first && Count > 0)
        {
            return Values.First();
        }

        return string.Empty;
    }
}
