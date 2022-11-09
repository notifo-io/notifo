// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Infrastructure.Reflection;
using Squidex.Assets;

namespace Notifo.Areas.Api.Controllers.Media.Dtos;

public sealed class MediaFileQueryDto
{
    private static readonly Dictionary<string, ResizeOptions> Presets = new Dictionary<string, ResizeOptions>(StringComparer.OrdinalIgnoreCase)
    {
        ["WebSmall"] = new ResizeOptions
        {
            TargetWidth = 192,
            TargetHeight = 192,
            Mode = ResizeMode.Crop
        },
        ["WebLarge"] = new ResizeOptions
        {
            TargetWidth = 360,
            TargetHeight = 180,
            Mode = ResizeMode.Crop
        },
        ["WebPushSmall"] = new ResizeOptions
        {
            TargetWidth = 192,
            TargetHeight = 192,
            Mode = ResizeMode.Crop
        },
        ["WebPushLarge"] = new ResizeOptions
        {
            TargetWidth = 360,
            TargetHeight = 180,
            Mode = ResizeMode.Crop
        }
    };

    /// <summary>
    /// The cache duration.
    /// </summary>
    [FromQuery(Name = "cache")]
    public long CacheDuration { get; set; }

    /// <summary>
    /// Set it to 1 to create a download response.
    /// </summary>
    [FromQuery(Name = "download")]
    public int Download { get; set; } = 0;

    /// <summary>
    /// The target width when an image.
    /// </summary>
    [FromQuery(Name = "width")]
    public int? Width { get; set; }

    /// <summary>
    /// The target height when an image.
    /// </summary>
    [FromQuery(Name = "height")]
    public int? Height { get; set; }

    /// <summary>
    /// The target quality when an image.
    /// </summary>
    [FromQuery(Name = "quality")]
    public int? Quality { get; set; }

    /// <summary>
    /// A preset dimension.
    /// </summary>
    [FromQuery(Name = "preset")]
    public string? Preset { get; set; }

    /// <summary>
    /// The resize mode.
    /// </summary>
    [FromQuery(Name = "mode")]
    public ResizeMode? Mode { get; set; }

    /// <summary>
    /// The x position of the focues point.
    /// </summary>
    [FromQuery(Name = "focusX")]
    public float? FocusX { get; set; }

    /// <summary>
    /// The y position of the focues point.
    /// </summary>
    [FromQuery(Name = "focusY")]
    public float? FocusY { get; set; }

    /// <summary>
    /// True to resize it and clear the cache.
    /// </summary>
    [FromQuery(Name = "force")]
    public bool ForceResize { get; set; }

    /// <summary>
    /// True, to return an empty image on failure.
    /// </summary>
    [FromQuery(Name = "emptyOnFailure")]
    public bool EmptyOnFailure { get; set; }

    public ResizeOptions ToResizeOptions()
    {
        if (Preset != null && Presets.TryGetValue(Preset, out var result))
        {
            return result;
        }

        result = SimpleMapper.Map(this, new ResizeOptions());

        result.TargetWidth = Width;
        result.TargetHeight = Height;

        return result;
    }
}
