// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class IntegrationDefinitionDto
    {
        /// <summary>
        /// The title of the integration.
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// The logo URL for the integration.
        /// </summary>
        [Required]
        public string LogoUrl { get; set; }

        /// <summary>
        /// The optional description of the integration.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// The properties to configure.
        /// </summary>
        [Required]
        public List<IntegrationPropertyDto> Properties { get; set; }

        /// <summary>
        /// The features of the integration.
        /// </summary>
        [Required]
        public IReadOnlySet<string> Capabilities { get; set; }

        public static IntegrationDefinitionDto FromDomainObject(IntegrationDefinition source)
        {
            var result = SimpleMapper.Map(source, new IntegrationDefinitionDto());

            result.Properties = new List<IntegrationPropertyDto>();

            foreach (var property in source.Properties)
            {
                var propertyDto = SimpleMapper.Map(property, new IntegrationPropertyDto());

                result.Properties.Add(propertyDto);
            }

            return result;
        }
    }
}
