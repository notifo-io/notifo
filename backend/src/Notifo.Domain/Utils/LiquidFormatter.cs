// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Utils;

public static class LiquidFormatter
{
    public static string FormatLiquid(this string text, Dictionary<string, string?> properties)
    {
        if (text == null || string.IsNullOrWhiteSpace(text))
        {
            return text ?? string.Empty;
        }

        try
        {
            var (template, _) = TemplateCache.Parse(text);

            if (template == null)
            {
                return text;
            }

            var templateContext = new TemplateContext();

            if (properties != null)
            {
                foreach (var (key, value) in properties)
                {
                    templateContext.SetValue(key, value);
                }
            }

            return template.Render(templateContext);
        }
        catch
        {
            return text;
        }
    }

    public static LocalizedText FormatLiquid(this LocalizedText input, Dictionary<string, string?> properties)
    {
        if (input == null)
        {
            return null!;
        }

        var result = new LocalizedText(input);

        foreach (var (key, value) in input)
        {
            result[key] = value.FormatLiquid(properties);
        }

        return result;
    }
}
