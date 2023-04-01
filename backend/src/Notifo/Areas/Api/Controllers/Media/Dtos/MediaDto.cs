// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Media;
using Notifo.Infrastructure.Reflection;
using Squidex.Hosting;
using MediaItem = Notifo.Domain.Media.Media;

namespace Notifo.Areas.Api.Controllers.Media.Dtos;

public sealed class MediaDto
{
    /// <summary>
    /// The mime type.
    /// </summary>
    public string MimeType { get; set; }

    /// <summary>
    /// The file name.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Generated information about the file.
    /// </summary>
    public string FileInfo { get; set; }

    /// <summary>
    /// The url to the media item.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The size of the media file.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// The date time (ISO 8601) when the media has been created.
    /// </summary>
    public Instant Created { get; set; }

    /// <summary>
    /// The date time (ISO 8601) when the media has been updated.
    /// </summary>
    public Instant LastUpdate { get; set; }

    /// <summary>
    /// The type of the media.
    /// </summary>
    public MediaType Type { get; set; }

    /// <summary>
    /// Metadata about the media.
    /// </summary>
    public MediaMetadata Metadata { get; set; }

    public static MediaDto FromDomainObject(MediaItem source, string appId, IUrlGenerator urlGenerator)
    {
        var result = SimpleMapper.Map(source, new MediaDto());

        result.Url = urlGenerator.BuildUrl($"api/apps/{appId}/media/{source.FileName}", false);

        return result;
    }
}
