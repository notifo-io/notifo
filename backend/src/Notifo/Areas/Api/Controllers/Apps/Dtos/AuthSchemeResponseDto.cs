// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos;

public sealed class AuthSchemeResponseDto
{
    /// <summary>
    /// The auth scheme if configured.
    /// </summary>
    public AuthSchemeDto? Scheme { get; set; }

    public static AuthSchemeResponseDto FromDomainObject(App source)
    {
        var result = new AuthSchemeResponseDto();

        if (source.AuthScheme != null)
        {
            result.Scheme = AuthSchemeDto.FromDomainObject(source.AuthScheme);
        }

        return result;
    }
}
