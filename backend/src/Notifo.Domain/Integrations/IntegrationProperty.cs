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

        public string? DefaultValue { get; init; }

        public string? GetString(ConfiguredIntegration configured)
        {
            if (Type == IntegrationPropertyType.Text || Type == IntegrationPropertyType.MultilineText || Type == IntegrationPropertyType.Password)
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
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                    {
                        return parsed;
                    }
                }

                if (int.TryParse(DefaultValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed2))
                {
                    return parsed2;
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
                    if (bool.TryParse(value, out var parsed))
                    {
                        return parsed;
                    }

                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedNumber))
                    {
                        return parsedNumber == 1;
                    }
                }

                if (bool.TryParse(DefaultValue, out var parsed2))
                {
                    return parsed2;
                }

                if (int.TryParse(DefaultValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedNumber2))
                {
                    return parsedNumber2 == 1;
                }
            }

            return false;
        }
    }
}
