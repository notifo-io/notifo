// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Notifo.Areas.Api.Controllers.Media.Dtos;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;
using Squidex.Assets;

namespace Notifo.Areas.Api.Controllers.Media;

[ApiExplorerSettings(GroupName = "Media")]
public sealed class MediaProxyController : MediaBaseController
{
    private static readonly HashSet<string> SafeHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        HeaderNames.Accept,
        HeaderNames.AcceptCharset,
        HeaderNames.AcceptEncoding,
        HeaderNames.AcceptLanguage,
        HeaderNames.CacheControl
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
    /// <response code="200">Media returned.</response>.
    /// <response code="404">Media or app not found.</response>.
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

                if (response.IsSuccessStatusCode)
                {
                    return Redirect("~/Empty.png");
                }

                static string MimeType(HttpContentHeaders headers)
                {
                    var mimeType = headers.ContentType?.ToString();

                    return mimeType.OrDefault("application/octet-stream");
                }

                static string FileName(HttpContentHeaders headers)
                {
                    var mimeType = headers.ContentDisposition?.FileName;

                    return mimeType.OrDefault("file");
                }

                var source = new ResizeSource
                {
                    MimeType = MimeType(response.Content.Headers),
                    FileId = url.Sha256Base64(),
                    FileName = FileName(response.Content.Headers),
                    FileSize = response.Content.Headers.ContentLength,
                    OpenRead = async (stream, context, ct) =>
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
