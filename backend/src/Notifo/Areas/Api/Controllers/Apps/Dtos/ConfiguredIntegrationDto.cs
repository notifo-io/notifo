// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class ConfiguredIntegrationDto
    {
        /// <summary>
        /// The integration type.
        /// </summary>
        [Required]
        public string Type { get; set; }

        /// <summary>
        /// The configured properties.
        /// </summary>
        [Required]
        public ReadonlyDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// True when enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// True or false when only used for test or production mode.
        /// </summary>
        public bool? Test { get; set; }

        /// <summary>
        /// The javascript condition.
        /// </summary>
        public string? Condition { get; set; }

        /// <summary>
        /// The priority in which order the integrations must run.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// The status of the integration.
        /// </summary>
        [Required]
        public IntegrationStatus Status { get; set; }

        public static ConfiguredIntegrationDto FromDomainObject(ConfiguredIntegration source)
        {
            var result = SimpleMapper.Map(source, new ConfiguredIntegrationDto());

            return result;
        }
    }
}
