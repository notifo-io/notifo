// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;

namespace Notifo.Infrastructure;

public static class FileExtensions
{
    private static readonly string[] Extensions =
    {
        "bytes",
        "kB",
        "MB",
        "GB",
        "TB"
    };

    public static string ToReadableSize(this int value)
    {
        return ToReadableSize((long)value);
    }

    public static string ToReadableSize(this long value)
    {
        if (value < 0)
        {
            return string.Empty;
        }

        var d = (double)value;
        var u = 0;

        const int multiplier = 1024;

        while ((d >= multiplier || -d >= multiplier) && u < Extensions.Length - 1)
        {
            d /= multiplier;
            u++;
        }

        if (u >= Extensions.Length - 1)
        {
            u = Extensions.Length - 1;
        }

        return $"{Math.Round(d, 1).ToString(CultureInfo.InvariantCulture)} {Extensions[u]}";
    }
}
