// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Utils
{
    public static class Formatter
    {
        private static readonly Regex FormatPattern = new Regex(@"\{\{[\s]*(?<Path>[\w]+)[\s]*(\|[\s]*(?<Transform>[^\?}]+))?(\?[\s]*(?<Fallback>[^\}\s]+))?[\s]*\}\}", RegexOptions.Compiled);

        public static string Format(this string text, Dictionary<string, string?> properties)
        {
            if (text == null || string.IsNullOrWhiteSpace(text))
            {
                return text ?? string.Empty;
            }

            return FormatPattern.Replace(text, match =>
            {
                var path = match.Groups["Path"].Value;

                if (properties.TryGetValue(path, out var result) && result != null)
                {
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

                if (string.IsNullOrWhiteSpace(result))
                {
                    result = match.Groups["Fallback"].Value;
                }

                return result;
            });
        }

        public static LocalizedText Format(this LocalizedText input, Dictionary<string, string?> properties)
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
}
