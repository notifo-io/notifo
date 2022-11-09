// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
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
    [Required]
    public string MimeType { get; set; }

    /// <summary>
    /// The file name.
    /// </summary>
    [Required]
    public string FileName { get; set; }

    /// <summary>
    /// Generated information about the file.
    /// </summary>
    [Required]
    public string FileInfo { get; set; }

    /// <summary>
    /// The url to the media item.
    /// </summary>
    [Required]
    public string Url { get; set; }

    /// <summary>
    /// The size of the media file.
    /// </summary>
    [Required]
    public long FileSize { get; set; }

    /// <summary>
    /// The date time (ISO 8601) when the media has been created.
    /// </summary>
    [Required]
    public Instant Created { get; set; }

    /// <summary>
    /// The date time (ISO 8601) when the media has been updated.
    /// </summary>
    [Required]
    public Instant LastUpdate { get; set; }

    /// <summary>
    /// The type of the media.
    /// </summary>
    [Required]
    public MediaType Type { get; set; }

    /// <summary>
    /// Metadata about the media.
    /// </summary>
    [Required]
    public MediaMetadata Metadata { get; set; }

    public static MediaDto FromDomainObject(MediaItem source, string appId, IUrlGenerator urlGenerator)
    {
        var result = SimpleMapper.Map(source, new MediaDto());

        result.Url = urlGenerator.BuildUrl($"api/apps/{appId}/media/{source.FileName}", false);

        return result;
    }
}
