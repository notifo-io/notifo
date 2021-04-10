// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Integrations
{
    public sealed record IntegrationProperty(string Name, TntegrationPropertyType Type)
    {
        public string? Description { get; init; }

        public bool IsRequired { get; init; }

        public int MinValue { get; init; } = 0;

        public int MaxValue { get; init; } = int.MaxValue;

        public int MinLength { get; init; } = 0;

        public int MaxLength { get; init; } = int.MaxValue;

        public object? DefaultValue { get; init; }

        public string? GetString(ConfiguredIntegration configured)
        {
            static bool TryGetString(object? value, out string result)
            {
                result = null!;

                if (value is string typed)
                {
                    result = typed;
                    return true;
                }

                return false;
            }

            if (Type == TntegrationPropertyType.Text || Type == TntegrationPropertyType.Number)
            {
                if (configured.Properties.TryGetValue(Name, out var value))
                {
                    if (TryGetString(value, out var typed))
                    {
                        return typed;
                    }
                }

                if (TryGetString(value, out var defaultTyped))
                {
                    return defaultTyped;
                }
            }

            return null;
        }

        public int GetInt(ConfiguredIntegration configured)
        {
            static bool TryGetInt(object? value, out int result)
            {
                result = 0;

                if (value is int typedInt)
                {
                    result = typedInt;
                    return true;
                }

                if (value is long typedLong)
                {
                    result = (int)typedLong;
                    return true;
                }

                return false;
            }

            if (Type == TntegrationPropertyType.Text || Type == TntegrationPropertyType.Number)
            {
                if (configured.Properties.TryGetValue(Name, out var value))
                {
                    if (TryGetInt(value, out var typed))
                    {
                        return typed;
                    }
                }

                if (TryGetInt(DefaultValue, out var defaultTyped))
                {
                    return defaultTyped;
                }
            }

            return 0;
        }
    }
}