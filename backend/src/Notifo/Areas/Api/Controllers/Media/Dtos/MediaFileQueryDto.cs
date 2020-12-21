// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Notifo.Infrastructure.Reflection;
using Squidex.Assets;

namespace Notifo.Areas.Api.Controllers.Media.Dtos
{
    public class MediaFileQueryDto
    {
        private static readonly Dictionary<string, ResizeOptions> Presets = new Dictionary<string, ResizeOptions>(StringComparer.OrdinalIgnoreCase)
        {
            ["WebSmall"] = new ResizeOptions { Width = 192, Height = 192, Mode = ResizeMode.BoxPad },
            ["WebLarge"] = new ResizeOptions { Width = 360, Height = 180, Mode = ResizeMode.Crop },
            ["WebPushSmall"] = new ResizeOptions { Width = 192, Height = 192, Mode = ResizeMode.BoxPad },
            ["WebPushLarge"] = new ResizeOptions { Width = 360, Height = 180, Mode = ResizeMode.Crop }
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

        public ResizeOptions ToResizeOptions()
        {
            if (Preset != null && Presets.TryGetValue(Preset, out var result))
            {
                return result;
            }

            result = SimpleMapper.Map(this, new ResizeOptions());

            return result;
        }
    }
}
