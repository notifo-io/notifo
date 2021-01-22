// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Assets;

namespace Notifo.Domain.Media
{
    public sealed class DefaultMediaFileStore : IMediaFileStore
    {
        private readonly IAssetStore assetStore;

        public DefaultMediaFileStore(IAssetStore assetStore)
        {
            this.assetStore = assetStore;
        }

        public Task DownloadAsync(string appId, Media media, Stream stream, BytesRange range, CancellationToken ct)
        {
            var fileName = CreateFileName(appId, media);

            return assetStore.DownloadAsync(fileName, stream, range, ct);
        }

        public Task UploadAsync(string appId, Media media, Stream stream, CancellationToken ct)
        {
            var fileName = CreateFileName(appId, media);

            return assetStore.UploadAsync(fileName, stream, true, ct);
        }

        private static string CreateFileName(string appId, Media media)
        {
            return $"{appId}_{media.FileName}";
        }
    }
}
