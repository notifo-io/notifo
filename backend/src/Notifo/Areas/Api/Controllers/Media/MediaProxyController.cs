// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.Media.Dtos;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;
using NSwag.Annotations;
using Squidex.Assets;

#pragma warning disable SA1119 // Statement should not use unnecessary parenthesis

namespace Notifo.Areas.Api.Controllers.Media
{
    [OpenApiTag("Media")]
    public sealed class MediaProxyController : MediaBaseController
    {
        private readonly IHttpClientFactory httpClientFactory;

        public MediaProxyController(IAssetStore assetStore, IAssetThumbnailGenerator assetThumbnailGenerator,
            IHttpClientFactory httpClientFactory)
            : base(assetStore, assetThumbnailGenerator)
        {
            this.httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Download a media object.
        /// </summary>
        /// <param name="url">The app id where the media belongs to.</param>
        /// <param name="query">Additional query parameters.</param>
        /// <returns>
        /// 200 => Media returned.
        /// 404 => Media or app not found.
        /// </returns>
        [HttpGet("api/assets/proxy")]
        [AllowAnonymous]
        public async Task<IActionResult> ProxyImage([FromQuery] string url, [FromQuery] MediaFileQueryDto? query = null)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ValidationException("Invalid URL.");
            }

            using (var httpClient = httpClientFactory.CreateClient())
            {
                var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode);
                }

                var source = new ResizeSource
                {
                    FileId = url.Sha256Base64(),
                    FileName = (response.Content.Headers.ContentDisposition?.FileName).OrDefault("file"),
                    FileSize = (response.Content.Headers.ContentLength) ?? 0,
                    MimeType = (response.Content.Headers.ContentType?.ToString()).OrDefault("application/octet-stream"),
                    OpenRead = async (stream, ct) =>
                    {
                        await using (var sourceStream = await response.Content.ReadAsStreamAsync(ct))
                        {
                            await sourceStream.CopyToAsync(stream, ct);
                        }
                    }
                };

                return DeliverAsset(source, query);
            }
        }
    }
}
