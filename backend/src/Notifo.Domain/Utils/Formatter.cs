﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Utils;

public static partial class Formatter
{
    private static readonly Regex FormatRegex = FormatFactory();

    [GeneratedRegex("\\{\\{[\\s]*(?<Path>[^\\s]+)[\\s]*(\\|[\\s]*(?<Transform>[^\\?}]+))?(\\?[\\s]*(?<Fallback>[^\\}\\s]+))?[\\s]*\\}\\}", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    private static partial Regex FormatFactory();

    public static string Format(this string text, IReadOnlyDictionary<string, string?>? properties)
    {
        if (text == null || string.IsNullOrWhiteSpace(text))
        {
            return text ?? string.Empty;
        }

        return FormatRegex.Replace(text, match =>
        {
            var path = match.Groups["Path"].Value;

            var result = string.Empty;

            if (properties != null && properties.TryGetValue(path, out var temp) && temp != null)
            {
                result = temp;

                var transforms = match.Groups["Transform"].Value.Split('|', StringSplitOptions.RemoveEmptyEntries);

                foreach (var transform in transforms)
                {
                    switch (transform.Trim().ToLowerInvariant())
                    {
                        case "lower":
                            result = result.ToLowerInvariant();
                            break;
                        case "upper":
                            result = result.ToUpperInvariant();
                            break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(result) && match.Groups.ContainsKey("Fallback"))
            {
                result = match.Groups["Fallback"].Value;
            }

            return result;
        });
    }

    public static LocalizedText Format(this LocalizedText input, Dictionary<string, string?>? properties)
    {
        if (input == null)
        {
            return null!;
        }

        var result = new LocalizedText(input);

        foreach (var (key, value) in input)
        {
            result[key] = value.Format(properties);
        }

        return result;
    }
}
