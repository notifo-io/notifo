// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

using System.Globalization;

namespace Notifo.Domain.Utils;

public sealed record TemplateError(string Message, int Line = -1, int Column = -1)
{
    public static TemplateError? Parse(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        message = message.TrimEnd('.');

        var startIndex = message.IndexOf("at (", StringComparison.OrdinalIgnoreCase);
        if (startIndex < 0)
        {
            return new TemplateError($"{message}.");
        }

        var endIndex = message.IndexOf(')', startIndex + 4);
        if (endIndex < 0)
        {
            return new TemplateError($"{message}.");
        }

        var parts = message[(startIndex + 4)..endIndex].Split(':');

        if (parts.Length is < 1 or > 2)
        {
            return new TemplateError(message);
        }

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var line))
        {
            line = -1;
        }

        var column = -1;

        if (parts.Length == 2 && !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out column))
        {
            column = -1;
        }

        message = $"{message[..startIndex].Trim()}.";

        return new TemplateError(message, line, column);
    }
}
