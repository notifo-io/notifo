// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Apps;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class CreateIntegrationDto
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
        public IntegrationProperties Properties { get; set; }

        /// <summary>
        /// True when enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// True when used for test events.
        /// </summary>
        public bool? Test { get; set; }

        /// <summary>
        /// The priority in which order the integrations must run.
        /// </summary>
        public int Priority { get; set; }

        public UpsertAppIntegration ToUpdate()
        {
            var result = SimpleMapper.Map(this, new UpsertAppIntegration());

            result.Id = Guid.NewGuid().ToString();

            return result;
        }
    }
}
