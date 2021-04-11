// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Notifo.Infrastructure;
using Squidex.Assets;

namespace Notifo.Domain.Media
{
    public sealed class ImageMediaMetadataSource : IMediaMetadataSource
    {
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public ImageMediaMetadataSource(IAssetThumbnailGenerator assetThumbnailGenerator)
        {
            Guard.NotNull(assetThumbnailGenerator, nameof(assetThumbnailGenerator));

            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        public async Task EnhanceAsync(Media media, AssetFile file)
        {
            if (media.Type == MediaType.Unknown)
            {
                await using (var uploadStream = file.OpenRead())
                {
                    var imageInfo = await assetThumbnailGenerator.GetImageInfoAsync(uploadStream);

                    if (imageInfo != null)
                    {
                        media.Type = MediaType.Image;

                        media.Metadata.SetPixelWidth(imageInfo.PixelWidth);
                        media.Metadata.SetPixelHeight(imageInfo.PixelHeight);
                    }
                }
            }
        }

        public IEnumerable<string> Format(Media media)
        {
            if (media.Type == MediaType.Image)
            {
                var w = media.Metadata.GetPixelWidth();
                var h = media.Metadata.GetPixelHeight();

                if (w != null && h != null)
                {
                    yield return $"{w}x{h}px";
                }
            }
        }
    }
}
