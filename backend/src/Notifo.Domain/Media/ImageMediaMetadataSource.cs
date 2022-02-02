// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;
using Squidex.Assets;

namespace Notifo.Domain.Media
{
    public sealed class ImageMediaMetadataSource : IMediaMetadataSource
    {
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public ImageMediaMetadataSource(IAssetThumbnailGenerator assetThumbnailGenerator)
        {
            Guard.NotNull(assetThumbnailGenerator);

            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        public async Task EnhanceAsync(MetadataRequest request)
        {
            var file = request.File;

            if (request.Type != MediaType.Unknown)
            {
                return;
            }

            var mimeType = file.MimeType;

            ImageInfo? imageInfo = null;

            await using (var uploadStream = file.OpenRead())
            {
                imageInfo = await assetThumbnailGenerator.GetImageInfoAsync(uploadStream, mimeType);
            }

            if (imageInfo != null)
            {
                var isSwapped = imageInfo.Orientation > ImageOrientation.TopLeft;

                if (isSwapped)
                {
                    var tempFile = TempAssetFile.Create(file);

                    await using (var uploadStream = file.OpenRead())
                    {
                        await using (var tempStream = tempFile.OpenWrite())
                        {
                            await assetThumbnailGenerator.FixOrientationAsync(uploadStream, mimeType, tempStream);
                        }
                    }

                    await using (var tempStream = tempFile.OpenRead())
                    {
                        imageInfo = await assetThumbnailGenerator.GetImageInfoAsync(tempStream, mimeType) ?? imageInfo;
                    }

                    await file.DisposeAsync();

                    request.File = tempFile;
                }
            }

            if (imageInfo != null)
            {
                request.Type = MediaType.Image;

                request.Metadata.SetPixelWidth(imageInfo.PixelWidth);
                request.Metadata.SetPixelHeight(imageInfo.PixelHeight);
            }
        }

        public IEnumerable<string> Format(MetadataRequest request)
        {
            if (request.Type == MediaType.Image)
            {
                var w = request.Metadata.GetPixelWidth();
                var h = request.Metadata.GetPixelHeight();

                if (w != null && h != null)
                {
                    yield return $"{w}x{h}px";
                }
            }
        }
    }
}
