// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Integrations;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos;

public sealed class ConfiguredIntegrationsDto
{
    /// <summary>
    /// The configured integrations.
    /// </summary>
    public Dictionary<string, ConfiguredIntegrationDto> Configured { get; set; }

    /// <summary>
    /// The supported integrations.
    /// </summary>
    public Dictionary<string, IntegrationDefinitionDto> Supported { get; set; }

    public static ConfiguredIntegrationsDto FromDomainObject(App source, IIntegrationManager integrationManager)
    {
        var result = new ConfiguredIntegrationsDto
        {
            Configured = new Dictionary<string, ConfiguredIntegrationDto>()
        };

        if (source.Integrations != null)
        {
            foreach (var (id, configured) in source.Integrations)
            {
                result.Configured[id] = ConfiguredIntegrationDto.FromDomainObject(configured);
            }
        }

        result.Supported = new Dictionary<string, IntegrationDefinitionDto>();

        foreach (var definition in integrationManager.Definitions)
        {
            result.Supported[definition.Type] = IntegrationDefinitionDto.FromDomainObject(definition);
        }

        return result;
    }
}
