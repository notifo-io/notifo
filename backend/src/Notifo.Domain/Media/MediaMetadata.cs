// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;

namespace Notifo.Domain.Media;

public sealed class MediaMetadata : Dictionary<string, string>
{
    public MediaMetadata SetPixelWidth(int value)
    {
        this["pixelWidth"] = value.ToString(CultureInfo.InvariantCulture);

        return this;
    }

    public MediaMetadata SetPixelHeight(int value)
    {
        this["pixelHeight"] = value.ToString(CultureInfo.InvariantCulture);

        return this;
    }

    public int? GetPixelWidth()
    {
        if (TryGetValue("pixelWidth", out var n) && int.TryParse(n, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return null;
    }

    public int? GetPixelHeight()
    {
        if (TryGetValue("pixelHeight", out var n) && int.TryParse(n, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return null;
    }
}
