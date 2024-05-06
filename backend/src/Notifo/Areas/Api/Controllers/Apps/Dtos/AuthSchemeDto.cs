// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Areas.Api.OpenApi;
using Notifo.Domain.Apps;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos;

[OpenApiRequest]
public sealed class AuthSchemeDto
{
    /// <summary>
    /// The domain name of your user accounts.
    /// </summary>
    [Required]
    public string Domain { get; init; }

    /// <summary>
    /// The display name for buttons.
    /// </summary>
    [Required]
    public string DisplayName { get; init; }

    /// <summary>
    /// The client ID.
    /// </summary>
    [Required]
    public string ClientId { get; init; }

    /// <summary>
    /// The client secret.
    /// </summary>
    [Required]
    public string ClientSecret { get; init; }

    /// <summary>
    /// The authority URL.
    /// </summary>
    [Required]
    public string Authority { get; init; }

    /// <summary>
    /// The URL to redirect after a signout.
    /// </summary>
    public string? SignoutRedirectUrl { get; init; }

    public AppAuthScheme ToDomainObject()
    {
        return SimpleMapper.Map(this, new AppAuthScheme());
    }

    public static AuthSchemeDto FromDomainObject(AppAuthScheme source)
    {
        return SimpleMapper.Map(source, new AuthSchemeDto());
    }
}
