// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;

namespace Notifo.Domain.Media
{
    public interface IMediaFileStore
    {
        Task DownloadAsync(string appId, Media media, Stream stream, BytesRange range,
            CancellationToken ct = default);

        Task UploadAsync(string appId, Media media, Stream stream,
            CancellationToken ct = default);
    }
}
