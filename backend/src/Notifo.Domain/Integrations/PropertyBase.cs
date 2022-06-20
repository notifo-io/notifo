// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

using System.Globalization;

namespace Notifo.Domain.Integrations
{
    public abstract record PropertyBase(string Name, PropertyType Type)
    {
        public string? DefaultValue { get; init; }

        public string? GetString(IReadOnlyDictionary<string, string>? properties)
        {
            if (Type is PropertyType.Text or PropertyType.MultilineText or PropertyType.Password)
            {
                if (properties != null && properties.TryGetValue(Name, out var value))
                {
                    return value;
                }

                return DefaultValue;
            }

            return null;
        }

        public long GetNumber(IReadOnlyDictionary<string, string>? properties)
        {
            if (Type == PropertyType.Number)
            {
                if (properties != null && properties.TryGetValue(Name, out var value))
                {
                    if (TryParseLong(value, out var result))
                    {
                        return result;
                    }
                }

                if (TryParseLong(DefaultValue, out var defaultResult))
                {
                    return defaultResult;
                }
            }

            return 0;
        }

        public bool GetBoolean(IReadOnlyDictionary<string, string>? properties)
        {
            if (Type == PropertyType.Boolean)
            {
                if (properties != null && properties.TryGetValue(Name, out var value))
                {
                    if (TryParseBoolean(value, out var result))
                    {
                        return result;
                    }
                }

                if (TryParseBoolean(DefaultValue, out var defaultResult))
                {
                    return defaultResult;
                }
            }

            return false;
        }

        private static bool TryParseLong(string? value, out long result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = 0;
                return true;
            }

            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseBoolean(string? value, out bool result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = false;
                return true;
            }

            if (bool.TryParse(value, out result))
            {
                return true;
            }

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedNumber))
            {
                result = parsedNumber == 1;
                return true;
            }

            return false;
        }
    }
}
