// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Notifo.Areas.Api.Controllers.Media.Dtos;
using Notifo.Domain;
using Notifo.Domain.Media;
using Notifo.Infrastructure.Validation;
using Notifo.Pipeline;
using NSwag.Annotations;
using Squidex.Assets;
using Squidex.Log;
using MediaItem = Notifo.Domain.Media.Media;

namespace Notifo.Areas.Api.Controllers.Media
{
    [OpenApiTag("Media")]
    public sealed class MediaController : BaseController
    {
        private readonly IAssetStore assetStore;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly IMediaStore mediaStore;
        private readonly IMediaFileStore mediaFileStore;

        public MediaController(
            IAssetStore assetStore,
            IAssetThumbnailGenerator assetThumbnailGenerator,
            IMediaStore mediaStore,
            IMediaFileStore mediaFileStore)
        {
            this.assetStore = assetStore;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
            this.mediaStore = mediaStore;
            this.mediaFileStore = mediaFileStore;
        }

        /// <summary>
        /// Query media items.
        /// </summary>
        /// <param name="appId">The app where the media belongs to.</param>
        /// <param name="q">The query object.</param>
        /// <returns>
        /// 200 => Media returned.
        /// </returns>
        [HttpGet("api/apps/{appId}/media/")]
        [AppPermission(Roles.Admin)]
        [Produces(typeof(ListResponseDto<MediaDto>))]
        public async Task<IActionResult> GetMedias(string appId, [FromQuery] QueryDto q)
        {
            var medias = await mediaStore.QueryAsync(appId, q.ToQuery<MediaQuery>(), HttpContext.RequestAborted);

            var response = new ListResponseDto<MediaDto>();

            response.Items.AddRange(medias.Select(MediaDto.FromDomainObject));
            response.Total = medias.Total;

            return Ok(response);
        }

        /// <summary>
        /// Download a media object.
        /// </summary>
        /// <param name="appId">The app id where the media belongs to.</param>
        /// <param name="fileName">The name of the media to download.</param>
        /// <param name="query">Additional query parameters.</param>
        /// <returns>
        /// 200 => Media returned.
        /// 404 => Media does not exist.
        /// </returns>
        [HttpGet("api/assets/{appId}/{fileName}")]
        [AllowAnonymous]
        public async Task<IActionResult> Download(string appId, string fileName, [FromQuery] MediaFileQueryDto? query = null)
        {
            var media = await mediaStore.GetAsync(appId, fileName, HttpContext.RequestAborted);

            return DeliverAsset(appId, media, query);
        }

        /// <summary>
        /// Upload a media object.
        /// </summary>
        /// <param name="appId">The app id where the media belongs to.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns>
        /// 201 => Media downloaded.
        /// </returns>
        [HttpPost("api/apps/{appId}/media/")]
        [AppPermission(Roles.Admin)]
        public async Task<IActionResult> Upload(string appId, IFormFile file)
        {
            var assetFile = CreateFile(file);

            await mediaStore.UploadAsync(appId, assetFile, HttpContext.RequestAborted);

            return CreatedAtAction(nameof(Download), new { appId, fileName = file.FileName }, null);
        }

        /// <summary>
        /// Delete a media.
        /// </summary>
        /// <param name="appId">The app id where the media belongs to.</param>
        /// <param name="fileName">The file name of the media.</param>
        /// <returns>
        /// 204 => Media deleted.
        /// </returns>
        [HttpDelete("api/apps/{appId}/media/{fileName}")]
        [AppPermission(Roles.Admin)]
        public async Task<IActionResult> Delete(string appId, string fileName)
        {
            await mediaStore.DeleteAsync(appId, fileName, HttpContext.RequestAborted);

            return NoContent();
        }

        private IActionResult DeliverAsset(string appId, MediaItem? media, MediaFileQueryDto? query)
        {
            query ??= new MediaFileQueryDto();

            if (media == null)
            {
                return NotFound();
            }

            var resizeOptions = query.ToResizeOptions();

            FileCallback callback;

            if (query.CacheDuration > 0)
            {
                Response.Headers[HeaderNames.CacheControl] = $"public,max-age={query.CacheDuration}";
            }

            var contentLength = (long?)null;

            if (media.Type == MediaType.Image && resizeOptions.IsValid)
            {
                callback = async (bodyStream, range, ct) =>
                {
                    var resizedAsset = $"{appId}_{media.FileName}_{resizeOptions}";

                    if (query.ForceResize)
                    {
                        await ResizeAsync(appId, media, bodyStream, resizedAsset, resizeOptions, true, ct);
                    }
                    else
                    {
                        try
                        {
                            await assetStore.DownloadAsync(resizedAsset, bodyStream, ct: ct);
                        }
                        catch (AssetNotFoundException)
                        {
                            await ResizeAsync(appId, media, bodyStream, resizedAsset, resizeOptions, false, ct);
                        }
                    }
                };
            }
            else
            {
                contentLength = media.FileSize;

                callback = async (bodyStream, range, ct) =>
                {
                    await mediaFileStore.DownloadAsync(appId, media, bodyStream, range, ct);
                };
            }

            return new FileCallbackResult(media.MimeType, callback)
            {
                EnableRangeProcessing = contentLength > 0,
                ErrorAs404 = true,
                FileDownloadName = media.FileName,
                FileSize = contentLength,
                SendInline = query.Download != 1
            };
        }

        private async Task ResizeAsync(string appId, MediaItem media, Stream bodyStream, string fileName, ResizeOptions resizeOptions, bool overwrite, CancellationToken ct)
        {
            using (Profiler.Trace("Resize"))
            {
                using (var sourceStream = GetTempStream())
                {
                    using (var destinationStream = GetTempStream())
                    {
                        using (Profiler.Trace("ResizeDownload"))
                        {
                            await mediaFileStore.DownloadAsync(appId, media, sourceStream, default, ct);
                            sourceStream.Position = 0;
                        }

                        using (Profiler.Trace("ResizeImage"))
                        {
                            await assetThumbnailGenerator.CreateThumbnailAsync(sourceStream, destinationStream, resizeOptions);
                            destinationStream.Position = 0;
                        }

                        using (Profiler.Trace("ResizeUpload"))
                        {
                            await assetStore.UploadAsync(fileName, destinationStream, overwrite, ct);
                            destinationStream.Position = 0;
                        }

                        await destinationStream.CopyToAsync(bodyStream, ct);
                    }
                }
            }
        }

        private static FileStream GetTempStream()
        {
            var tempFileName = Path.GetTempFileName();

            const int bufferSize = 16 * 1024;

            return new FileStream(tempFileName,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Delete,
                bufferSize,
                FileOptions.Asynchronous |
                FileOptions.DeleteOnClose |
                FileOptions.SequentialScan);
        }

        private static AssetFile CreateFile(IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(file?.ContentType))
            {
                throw new ValidationException("File content-type is not defined.");
            }

            return new DelegateAssetFile(file.FileName, file.ContentType, file.Length, file.OpenReadStream);
        }
    }
}
