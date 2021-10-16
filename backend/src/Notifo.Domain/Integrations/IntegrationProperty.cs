// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Notifo.Domain.Resources;

namespace Notifo.Domain.Integrations
{
    public sealed record IntegrationProperty(string Name, IntegrationPropertyType Type)
    {
        public string? EditorDescription { get; init; }

        public string? EditorLabel { get; init; }

        public string[]? AllowedValues { get; init; }

        public bool Summary { get; init; }

        public bool IsRequired { get; init; }

        public int MinValue { get; init; }

        public int MaxValue { get; init; } = int.MaxValue;

        public int MinLength { get; init; }

        public int MaxLength { get; init; } = int.MaxValue;

        public string? Pattern { get; init; }

        public string? DefaultValue { get; init; }

        public IEnumerable<string> Validate(string? value)
        {
            switch (Type)
            {
                case IntegrationPropertyType.Boolean:
                    return Enumerable.Empty<string>();
                case IntegrationPropertyType.Number:
                    return ValidateNumber(value);
                default:
                    return ValidateString(value);
            }
        }

        private IEnumerable<string> ValidateNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) && IsRequired)
            {
                yield return Texts.IntegrationPropertyRequired;
            }

            if (!TryParseInt(value, out var number))
            {
                yield return Texts.IntegrationPropertyInvalidNumber;
            }

            if (number < MinValue)
            {
                yield return string.Format(CultureInfo.InvariantCulture, Texts.IntegrationPropertyMinValue, MinValue);
            }

            if (number > MaxValue)
            {
                yield return string.Format(CultureInfo.InvariantCulture, Texts.IntegrationPropertyMaxValue, MaxValue);
            }
        }

        private IEnumerable<string> ValidateString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) && IsRequired)
            {
                yield return Texts.IntegrationPropertyRequired;
            }

            var length = value?.Length ?? 0;

            if (length < MinLength)
            {
                yield return string.Format(CultureInfo.InvariantCulture, Texts.IntegrationPropertyMinLength, MinLength);
            }

            if (length > MaxLength)
            {
                yield return string.Format(CultureInfo.InvariantCulture, Texts.IntegrationPropertyMaxLength, MaxLength);
            }

            if (value != null && Pattern != null)
            {
                bool isValid;
                try
                {
                    isValid = Regex.IsMatch(value, Pattern);
                }
                catch (ArgumentException)
                {
                    isValid = false;
                }

                if (!isValid)
                {
                    yield return Texts.IntegrationPropertyPattern;
                }
            }
        }

        public string? GetString(ConfiguredIntegration configured)
        {
            if (Type is IntegrationPropertyType.Text or IntegrationPropertyType.MultilineText or IntegrationPropertyType.Password)
            {
                if (configured.Properties.TryGetValue(Name, out var value))
                {
                    return value;
                }

                return DefaultValue;
            }

            return null;
        }

        public int GetInt(ConfiguredIntegration configured)
        {
            if (Type == IntegrationPropertyType.Number)
            {
                if (configured.Properties.TryGetValue(Name, out var value))
                {
                    if (TryParseInt(value, out var result))
                    {
                        return result;
                    }
                }

                if (TryParseInt(DefaultValue, out var defaultResult))
                {
                    return defaultResult;
                }
            }

            return 0;
        }

        public bool GetBoolean(ConfiguredIntegration configured)
        {
            if (Type == IntegrationPropertyType.Boolean)
            {
                if (configured.Properties.TryGetValue(Name, out var value))
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

        private static bool TryParseInt(string? value, out int result)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = 0;
                return true;
            }

            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseBoolean(string? value, out bool result)
        {
            if (!string.IsNullOrWhiteSpace(value))
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
