// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure.Validation;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Integrations;

public sealed record IntegrationProperty(string Name, PropertyType Type)
{
    public string? DefaultValue { get; init; }

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

    public bool IsValid(string? input, [MaybeNullWhen(true)] out string error)
    {
        switch (Type)
        {
            case PropertyType.Boolean:
                return TryGetBoolean(input, out error, out _);
            case PropertyType.Number:
                return TryGetNumber(input, out error, out _);
            default:
                return TryGetString(input, out error, out _);
        }
    }

    public string GetString(IReadOnlyDictionary<string, string>? properties)
    {
        if (Type is PropertyType.Text or PropertyType.MultilineText or PropertyType.Password)
        {
            string? input = null;

            properties?.TryGetValue(Name, out input);

            if (!TryGetString(input, out var error, out var result))
            {
                throw new ValidationException(new ValidationError(error, Name));
            }

            return result;
        }

        throw new ValidationException(Texts.IntegrationPropertyNotString);
    }

    public long GetNumber(IReadOnlyDictionary<string, string>? properties)
    {
        if (Type is PropertyType.Number)
        {
            string? input = null;

            properties?.TryGetValue(Name, out input);

            if (!TryGetNumber(input, out var error, out var result))
            {
                throw new ValidationException(new ValidationError(error, Name));
            }

            return result;
        }

        throw new ValidationException(Texts.IntegrationPropertyNotNumber);
    }

    public bool GetBoolean(IReadOnlyDictionary<string, string>? properties)
    {
        if (Type is PropertyType.Boolean)
        {
            string? input = null;

            properties?.TryGetValue(Name, out input);

            if (!TryGetBoolean(input, out var error, out var result))
            {
                throw new ValidationException(new ValidationError(error, Name));
            }

            return result;
        }

        throw new ValidationException(Texts.IntegrationPropertyNotBoolean);
    }

    private bool TryGetString(string? input, [MaybeNullWhen(true)] out string error, out string result)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            input = DefaultValue;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            input = AllowedValues?.FirstOrDefault();
        }

        result = input ?? string.Empty;

        error = null!;

        if (string.IsNullOrWhiteSpace(input) && IsRequired)
        {
            error = Texts.IntegrationPropertyRequired;
            return false;
        }

        var length = input?.Length ?? 0;

        if (length < MinLength)
        {
            error = string.Format(CultureInfo.InvariantCulture, Texts.IntegrationPropertyMinLength, MinLength);
            return false;
        }

        if (length > MaxLength)
        {
            error = string.Format(CultureInfo.InvariantCulture, Texts.IntegrationPropertyMaxLength, MaxLength);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(input) && AllowedValues?.Contains(input) == false)
        {
            error = Texts.IntegrationPropertyAllowedValue;
            return false;
        }

        if (!string.IsNullOrWhiteSpace(input) && Pattern != null)
        {
            bool isValid;
            try
            {
                isValid = Regex.IsMatch(input, Pattern);
            }
            catch (ArgumentException)
            {
                isValid = false;
            }

            if (!isValid)
            {
                error = Texts.IntegrationPropertyPattern;
                return false;
            }
        }

        return true;
    }

    private bool TryGetNumber(string? input, out string error, out long result)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            input = DefaultValue;
        }

        result = 0;

        error = null!;

        if (string.IsNullOrWhiteSpace(input) && IsRequired)
        {
            error = Texts.IntegrationPropertyRequired;
            return false;
        }

        if (!TryParseLong(input, out result))
        {
            error = Texts.IntegrationPropertyInvalidNumber;
            return false;
        }

        if (result < MinValue)
        {
            error = string.Format(CultureInfo.InvariantCulture, Texts.IntegrationPropertyMinValue, MinValue);
            return false;
        }

        if (result > MaxValue)
        {
            error = string.Format(CultureInfo.InvariantCulture, Texts.IntegrationPropertyMaxValue, MaxValue);
            return false;
        }

        return true;
    }

    private bool TryGetBoolean(string? input, [MaybeNullWhen(true)] out string error, out bool result)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            input = DefaultValue;
        }

        result = false;

        error = null!;

        if (string.IsNullOrWhiteSpace(input) && IsRequired)
        {
            error = Texts.IntegrationPropertyRequired;
            return false;
        }

        if (!TryParseBoolean(input, out result))
        {
            error = Texts.IntegrationPropertyInvalidBoolean;
            return false;
        }

        return true;
    }

    private static bool TryParseLong(string? value, out long result)
    {
        result = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
        {
            return true;
        }

        return false;
    }

    private static bool TryParseBoolean(string? value, out bool result)
    {
        result = false;

        if (string.IsNullOrWhiteSpace(value))
        {
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
