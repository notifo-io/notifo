// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain;
using Notifo.Infrastructure.Reflection;
using Notifo.Infrastructure.Texts;

namespace Notifo.Areas.Api.Controllers
{
    public sealed class NotificationFormattingDto
    {
        /// <summary>
        /// The required subject with one entry per language.
        /// </summary>
        public LocalizedText Subject { get; set; }

        /// <summary>
        /// The optional body with one entry per language.
        /// </summary>
        public LocalizedText? Body { get; set; }

        /// <summary>
        /// The optional confirm text with one entry per language.
        /// </summary>
        public LocalizedText? ConfirmText { get; set; }

        /// <summary>
        /// The optional small image with one entry per language.
        /// </summary>
        public LocalizedText? ImageSmall { get; set; }

        /// <summary>
        /// The optional large image with one entry per language.
        /// </summary>
        public LocalizedText? ImageLarge { get; set; }

        /// <summary>
        /// The optional link url with one entry per language.
        /// </summary>
        public LocalizedText? LinkUrl { get; set; }

        /// <summary>
        /// The optional link name with one entry per language.
        /// </summary>
        public LocalizedText? LinkText { get; set; }

        /// <summary>
        /// The confirmation mode.
        /// </summary>
        public ConfirmMode ConfirmMode { get; set; }

        public static NotificationFormattingDto FromDomainObject(NotificationFormatting<LocalizedText> source)
        {
            var result = SimpleMapper.Map(source, new NotificationFormattingDto());

            return result;
        }

        public NotificationFormatting<LocalizedText> ToDomainObject()
        {
            var result = SimpleMapper.Map(this, new NotificationFormatting<LocalizedText>());

            return result;
        }
    }
}
