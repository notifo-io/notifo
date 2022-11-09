// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Media;

public sealed record Media(string AppId, string FileName, Instant Created)
{
    public string MimeType { get; init; }

    public string FileInfo { get; init; }

    public long FileSize { get; init; }

    public Instant LastUpdate { get; init; }

    public MediaType Type { get; init; }

    public MediaMetadata Metadata { get; init; }
}
