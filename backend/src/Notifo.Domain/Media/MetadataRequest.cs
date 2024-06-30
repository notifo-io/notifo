// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;

namespace Notifo.Domain.Media;

public sealed class MetadataRequest
{
    public IAssetFile File { get; set; }

    public MediaMetadata Metadata { get; } = [];

    public MediaType Type { get; set; }
}
