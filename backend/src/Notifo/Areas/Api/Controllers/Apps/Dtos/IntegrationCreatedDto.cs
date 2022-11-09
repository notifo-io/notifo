// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Apps;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos;

public sealed class IntegrationCreatedDto
{
    /// <summary>
    /// The id of the integration.
    /// </summary>
    [Required]
    public string Id { get; set; }

    /// <summary>
    /// The integration.
    /// </summary>
    [Required]
    public ConfiguredIntegrationDto Integration { get; set; }

    public static IntegrationCreatedDto FromDomainObject(App source, string id)
    {
        return new IntegrationCreatedDto
        {
            Id = id,
            Integration = ConfiguredIntegrationDto.FromDomainObject(source.Integrations[id])
        };
    }
}
