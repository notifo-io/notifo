﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Media.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.Media;
using Notifo.Infrastructure.Validation;
using Notifo.Pipeline;
using Squidex.Assets;
using Squidex.Hosting;

namespace Notifo.Areas.Api.Controllers.Media;

[ApiExplorerSettings(GroupName = "Media")]
public sealed class MediaController : MediaBaseController
{
    private readonly IMediaStore mediaStore;
    private readonly IMediaFileStore mediaFileStore;
    private readonly IUrlGenerator urlGenerator;

    public MediaController(
        IAssetStore assetStore,
        IAssetThumbnailGenerator assetThumbnailGenerator,
        IMediaStore mediaStore,
        IMediaFileStore mediaFileStore,
        IUrlGenerator urlGenerator)
        : base(assetStore, assetThumbnailGenerator)
    {
        this.mediaStore = mediaStore;
        this.mediaFileStore = mediaFileStore;
        this.urlGenerator = urlGenerator;
    }

    /// <summary>
    /// Query media items.
    /// </summary>
    /// <param name="appId">The app where the media belongs to.</param>
    /// <param name="q">The query object.</param>
    /// <response code="200">Media returned.</response>.
    /// <response code="404">App not found.</response>.
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
    /// <response code="200">Media returned.</response>.
    /// <response code="404">Media or app not found.</response>.
    [HttpGet("api/apps/{appId:notEmpty}/media/{fileName:notEmpty}")]
    [HttpGet("api/asset/{appId:notEmpty}/{fileName:notEmpty}")]
    [HttpGet("api/assets/{appId:notEmpty}/{fileName:notEmpty}")]
    [AllowAnonymous]
    public async Task<IActionResult> Download(string appId, string fileName, [FromQuery] MediaFileQueryDto? query = null)
    {
        var media = await mediaStore.GetAsync(appId, fileName, HttpContext.RequestAborted);

        if (media == null)
        {
            return NotFound();
        }

        try
        {
            var source = new ResizeSource
            {
                MimeType = media.MimeType,
                FileId = $"{appId}_{media.FileName}",
                FileName = media.FileName,
                FileSize = media.FileSize,
                OpenRead = (stream, context, ct) =>
                {
                    return mediaFileStore.DownloadAsync(appId, media, stream, default, ct);
                }
            };

            return DeliverAssetAsync(source, query);
        }
        catch when (query?.EmptyOnFailure == true)
        {
            return new VirtualFileResult("~/Empty.png", "image/png");
        }
    }

    /// <summary>
    /// Upload a media object.
    /// </summary>
    /// <param name="appId">The app id where the media belongs to.</param>
    /// <param name="file">The file to upload.</param>
    /// <response code="201">Media uploaded.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost("api/apps/{appId:notEmpty}/media/")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status201Created)]
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
    /// <response code="204">Media deleted.</response>.
    /// <response code="404">App not found.</response>.
    [HttpDelete("api/apps/{appId:notEmpty}/media/{fileName:notEmpty}")]
    [AppPermission(NotifoRoles.AppAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(string appId, string fileName)
    {
        await mediaStore.DeleteAsync(appId, fileName, HttpContext.RequestAborted);

        return NoContent();
    }

    private static DelegateAssetFile CreateFile(IFormFile file)
    {
        if (string.IsNullOrWhiteSpace(file?.ContentType))
        {
            throw new ValidationException("File content-type is not defined.");
        }

        return new DelegateAssetFile(file.FileName, file.ContentType, file.Length, file.OpenReadStream);
    }
}
