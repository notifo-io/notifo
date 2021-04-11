// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Notifo.Domain.Integrations;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class IntegrationPropertyDto
    {
        /// <summary>
        /// The field name for the property.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The editor type.
        /// </summary>
        [Required]
        public IntegrationPropertyType Type { get; set; }

        /// <summary>
        /// The optional description.
        /// </summary>
        public string? EditorDescription { get; set; }

        /// <summary>
        /// The optional label.
        /// </summary>
        public string? EditorLabel { get; set; }

        /// <summary>
        /// True to show this property in the summary.
        /// </summary>
        public bool Summary { get; set; }

        /// <summary>
        /// True when required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// The min value (for numbers).
        /// </summary>
        public int MinValue { get; set; }

        /// <summary>
        /// The max value (for numbers).
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// The min length (for strings).
        /// </summary>
        public int MinLength { get; set; }

        /// <summary>
        /// The min length (for strings).
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// The default value.
        /// </summary>
        public object? DefaultValue { get; set; }
    }
}
