// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;
using Squidex.Assets;

namespace Notifo.Domain.Media
{
    public sealed class MediaStore : IMediaStore
    {
        private readonly IMediaFileStore mediaFileStore;
        private readonly IMediaRepository mediaRepository;
        private readonly IEnumerable<IMediaMetadataSource> mediaMetadataSources;

        public MediaStore(
            IMediaFileStore mediaFileStore,
            IMediaRepository mediaRepository,
            IEnumerable<IMediaMetadataSource> mediaMetadataSources)
        {
            this.mediaFileStore = mediaFileStore;
            this.mediaMetadataSources = mediaMetadataSources;
            this.mediaRepository = mediaRepository;
        }

        public Task<IResultList<Media>> QueryAsync(string appId, MediaQuery query, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNull(query, nameof(query));

            return mediaRepository.QueryAsync(appId, query, ct);
        }

        public async Task<Media> UploadAsync(string appId, AssetFile file, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNull(file, nameof(file));

            var media = new Media
            {
                AppId = appId,
                FileName = file.FileName,
                FileSize = file.FileSize,
                MimeType = file.MimeType,
                Metadata = new MediaMetadata()
            };

            foreach (var metadataSource in mediaMetadataSources)
            {
                await metadataSource.EnhanceAsync(media, file);
            }

            var infos = new List<string>();

            foreach (var metadataSource in mediaMetadataSources)
            {
                infos.AddRange(metadataSource.Format(media));
            }

            media.FileInfo = string.Join(", ", infos);

            using (var stream = file.OpenRead())
            {
                await mediaFileStore.UploadAsync(appId, media, stream, ct);
            }

            await mediaRepository.UpsertAsync(media, ct);

            return media;
        }

        public Task<Media?> GetAsync(string appId, string fileName, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(fileName, nameof(fileName));

            return mediaRepository.GetAsync(appId, fileName, ct);
        }

        public Task DeleteAsync(string appId, string fileName, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(appId, nameof(appId));
            Guard.NotNullOrEmpty(fileName, nameof(fileName));

            return mediaRepository.DeleteAsync(appId, fileName, ct);
        }
    }
}
