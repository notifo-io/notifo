// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text.RegularExpressions;
using Notifo.Domain.Resources;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Integrations
{
    public sealed record IntegrationProperty(string Name, PropertyType Type) : PropertyBase(Name, Type)
    {
        public string? EditorDescription { get; init; }

        public string? EditorLabel { get; init; }

        public string[]? AllowedValues { get; init; }

        public bool Summary { get; init; }

        public bool IsRequired { get; init; }

        public long? MinValue { get; init; }

        public long? MaxValue { get; init; }

        public long? MinLength { get; init; }

        public long? MaxLength { get; init; }

        public string? Pattern { get; init; }

        public IEnumerable<string> Validate(string? value)
        {
            switch (Type)
            {
                case PropertyType.Boolean:
                    return Enumerable.Empty<string>();
                case PropertyType.Number:
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

            if (!TryParseLong(value, out var number))
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
            return GetString(configured.Properties);
        }

        public long GetNumber(ConfiguredIntegration configured)
        {
            return GetNumber(configured.Properties);
        }

        public bool GetBoolean(ConfiguredIntegration configured)
        {
            return GetBoolean(configured.Properties);
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
    }
}
