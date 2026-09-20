// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure;

public static class FieldLengths
{
    public const int Etag = 40;

    public const int AppId = 100;

    public const int Id = 200;

    public const int Key = 400;

    public const int Text = 1000;

    public const int LongText = 4000;

    public static string? ToMaxLength(this string? value, int maxLength)
    {
        // Only used for columns that are searched or displayed, but not for identifiers.
        return value?.Length > maxLength ? value[..maxLength] : value;
    }
}
