// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
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
        private static readonly HashSet<string> SafeHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            HeaderNames.Accept,
            HeaderNames.AcceptCharset,
            HeaderNames.AcceptEncoding,
            HeaderNames.AcceptLanguage,
            HeaderNames.CacheControl,
            HeaderNames.UserAgent
        };

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
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new ValidationException("Invalid URL.");
            }

            if (string.Equals(uri.Host, "via.placeholder.com", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(url);
            }

            using (var httpClient = httpClientFactory.CreateClient("Unsafe"))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                foreach (var (key, value) in Request.Headers)
                {
                    if (SafeHeaders.Contains(key))
                    {
                        request.Headers.TryAddWithoutValidation(key, (IEnumerable<string?>)value);
                    }
                }

                try
                {
                    var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);

                    if (!response.IsSuccessStatusCode)
                    {
                        return Redirect("~/Empty.png");
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

                    return DeliverAssetAsync(source, query);
                }
                catch
                {
                    return Redirect("~/Empty.png");
                }
            }
        }
    }
}
