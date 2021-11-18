// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;

namespace Notifo.Domain.Media
{
    public interface IMediaMetadataSource
    {
        Task EnhanceAsync(Media media, AssetFile file);

        IEnumerable<string> Format(Media media);
    }
}
