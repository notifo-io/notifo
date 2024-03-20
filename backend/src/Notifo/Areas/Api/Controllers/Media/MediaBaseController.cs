﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Notifo.Areas.Api.Controllers.Media.Dtos;
using Notifo.Infrastructure;
using Notifo.Pipeline;
using Squidex.Assets;

namespace Notifo.Areas.Api.Controllers.Media;

public abstract class MediaBaseController : BaseController
{
    private readonly IAssetStore assetStore;
    private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

    public sealed class ResizeSource
    {
        required public string FileId { get; init; }

        required public string FileName { get; init; }

        required public string MimeType { get; init; }

        required public long? FileSize { get; init; }

        required public Func<Stream, HttpContext, CancellationToken, Task> OpenRead { get; init; }
    }

    protected MediaBaseController(
        IAssetStore assetStore,
        IAssetThumbnailGenerator assetThumbnailGenerator)
    {
        this.assetStore = assetStore;
        this.assetThumbnailGenerator = assetThumbnailGenerator;
    }

    protected IActionResult DeliverAssetAsync(ResizeSource source, MediaFileQueryDto? query)
    {
        query ??= new MediaFileQueryDto();

        if (source == null)
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
        var contentType = source.MimeType;

        if (assetThumbnailGenerator.IsResizable(source.MimeType, resizeOptions, out var destinationMimeType))
        {
            if (destinationMimeType != null)
            {
                contentType = destinationMimeType;
            }

            contentCallback = async (bodyStream, context, range, ct) =>
            {
                var cacheId = $"{source.FileId}_{resizeOptions}";

                if (query.ForceResize)
                {
                    await ResizeAsync(source, contentType, context, bodyStream, cacheId, resizeOptions, true, ct);
                }
                else
                {
                    try
                    {
                        await assetStore.DownloadAsync(cacheId, bodyStream, ct: ct);
                    }
                    catch (AssetNotFoundException)
                    {
                        await ResizeAsync(source, contentType, context, bodyStream, cacheId, resizeOptions, false, ct);
                    }
                }
            };
        }
        else
        {
            contentLength = source.FileSize;

            contentCallback = async (bodyStream, context, range, ct) =>
            {
                await source.OpenRead(bodyStream, context, ct);
            };
        }

        return new FileCallbackResult(contentType, contentCallback)
        {
            EnableRangeProcessing = false,
            ErrorAs404 = true,
            FileDownloadName = source.FileName,
            FileSize = contentLength,
            SendInline = query.Download != 1
        };
    }

    private async Task ResizeAsync(ResizeSource source, string destinationContentType, HttpContext context, Stream target, string cacheId, ResizeOptions resizeOptions, bool overwrite,
        CancellationToken ct)
    {
#pragma warning disable MA0040 // Flow the cancellation token
        using var activity = Telemetry.Activities.StartActivity("Resize");

        await using var assetOriginal = new TempAssetFile(source.FileName, source.MimeType);
        await using var assetResized = new TempAssetFile(source.FileName, destinationContentType);

        using (Telemetry.Activities.StartActivity("Read"))
        {
            await using (var originalStream = assetOriginal.OpenWrite())
            {
                await source.OpenRead(originalStream, context, ct);
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
                        await assetThumbnailGenerator.CreateThumbnailAsync(originalStream, source.MimeType, resizeStream, resizeOptions);
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
                    await assetStore.UploadAsync(cacheId, resizeStream, overwrite);
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
}
