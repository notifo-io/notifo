// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos;

public sealed class AuthSchemeValueDto
{
    /// <summary>
    /// The auth scheme if configured.
    /// </summary>
    public AuthSchemeDto? Scheme { get; set; }

    public UpsertAppAuthScheme ToUpsert()
    {
        return new UpsertAppAuthScheme
        {
            Scheme = Scheme?.ToDomainObject()
        };
    }

    public static AuthSchemeValueDto FromDomainObject(App source)
    {
        var result = new AuthSchemeValueDto();

        if (source.AuthScheme != null)
        {
            result.Scheme = AuthSchemeDto.FromDomainObject(source.AuthScheme);
        }

        return result;
    }
}
