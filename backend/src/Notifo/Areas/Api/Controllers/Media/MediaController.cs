﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Notifo.Areas.Api.Controllers.Media.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.Media;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;
using Notifo.Pipeline;
using NSwag.Annotations;
using Squidex.Assets;
using Squidex.Hosting;
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
        private readonly IUrlGenerator urlGenerator;

        public MediaController(
            IAssetStore assetStore,
            IAssetThumbnailGenerator assetThumbnailGenerator,
            IMediaStore mediaStore,
            IMediaFileStore mediaFileStore,
            IUrlGenerator urlGenerator)
        {
            this.assetStore = assetStore;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
            this.mediaStore = mediaStore;
            this.mediaFileStore = mediaFileStore;
            this.urlGenerator = urlGenerator;
        }

        /// <summary>
        /// Query media items.
        /// </summary>
        /// <param name="appId">The app where the media belongs to.</param>
        /// <param name="q">The query object.</param>
        /// <returns>
        /// 200 => Media returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet("api/apps/{appId:notEmpty}/media/")]
        [AppPermission(NotifoRoles.AppAdmin)]
        [Produces(typeof(ListResponseDto<MediaDto>))]
        public async Task<IActionResult> GetMedias(string appId, [FromQuery] QueryDto q)
        {
            var medias = await mediaStore.QueryAsync(appId, q.ToQuery<MediaQuery>(true), HttpContext.RequestAborted);

            var response = new ListResponseDto<MediaDto>();

            response.Items.AddRange(medias.Select(x => MediaDto.FromDomainObject(x, appId, urlGenerator)));
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
        /// 404 => Media or app not found.
        /// </returns>
        [HttpGet("api/apps/{appId:notEmpty}/media/{fileName:notEmpty}")]
        [HttpGet("api/assets/{appId:notEmpty}/{fileName:notEmpty}")]
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
        /// 201 => Media uploaded.
        /// 404 => App not found.
        /// </returns>
        [HttpPost("api/apps/{appId:notEmpty}/media/")]
        [AppPermission(NotifoRoles.AppAdmin)]
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
        /// 404 => App not found.
        /// </returns>
        [HttpDelete("api/apps/{appId:notEmpty}/media/{fileName:notEmpty}")]
        [AppPermission(NotifoRoles.AppAdmin)]
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

            if (query.CacheDuration > 0)
            {
                Response.Headers[HeaderNames.CacheControl] = $"public,max-age={query.CacheDuration}";
            }

            var contentLength = (long?)null;
            var contentCallback = (FileCallback?)null;
            var contentType = media.MimeType;

            if (media.Type == MediaType.Image && assetThumbnailGenerator.IsResizable(media.MimeType, resizeOptions, out var destinationMimeType))
            {
                contentType = destinationMimeType;
                contentCallback = async (bodyStream, range, ct) =>
                {
                    var resizedAsset = $"{appId:notEmpty}_{media.FileName}_{resizeOptions:notEmpty}";

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

                contentCallback = async (bodyStream, range, ct) =>
                {
                    await mediaFileStore.DownloadAsync(appId, media, bodyStream, range, ct);
                };
            }

            return new FileCallbackResult(media.MimeType, contentCallback)
            {
                EnableRangeProcessing = contentLength > 0,
                ErrorAs404 = true,
                FileDownloadName = media.FileName,
                FileSize = contentLength,
                SendInline = query.Download != 1
            };
        }

        private async Task ResizeAsync(string appId, MediaItem media, Stream target, string fileName, ResizeOptions resizeOptions, bool overwrite,
            CancellationToken ct)
        {
#pragma warning disable MA0040 // Flow the cancellation token
            using var activity = Telemetry.Activities.StartActivity("Resize");

            await using var assetOriginal = new TempAssetFile(media.FileName, media.MimeType, 0);
            await using var assetResized = new TempAssetFile(media.FileName, media.MimeType, 0);

            using (Telemetry.Activities.StartActivity("Read"))
            {
                await using (var originalStream = assetOriginal.OpenWrite())
                {
                    await mediaFileStore.DownloadAsync(appId, media, originalStream, default, ct);
                }
            }

            using (Telemetry.Activities.StartActivity("Resize"))
            {
                try
                {
                    await using (var originalStream = assetOriginal.OpenRead())
                    {
                        await using (var resizeStream = assetResized.OpenWrite())
                        {
                            await assetThumbnailGenerator.CreateThumbnailAsync(originalStream, media.MimeType, resizeStream, resizeOptions);
                        }
                    }
                }
                catch
                {
                    await using (var originalStream = assetOriginal.OpenRead())
                    {
                        await using (var resizeStream = assetResized.OpenWrite())
                        {
                            await originalStream.CopyToAsync(resizeStream);
                        }
                    }
                }
            }

            using (Telemetry.Activities.StartActivity("Save"))
            {
                try
                {
                    await using (var resizeStream = assetResized.OpenRead())
                    {
                        await assetStore.UploadAsync(fileName, resizeStream, overwrite);
                    }
                }
                catch (AssetAlreadyExistsException)
                {
                    return;
                }
            }

            using (Telemetry.Activities.StartActivity("Write"))
            {
                await using (var resizeStream = assetResized.OpenRead())
                {
                    await resizeStream.CopyToAsync(target, ct);
                }
            }
#pragma warning restore MA0040 // Flow the cancellation token
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
