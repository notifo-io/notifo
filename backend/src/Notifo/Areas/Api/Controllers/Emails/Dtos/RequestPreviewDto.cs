// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.Emails.Dtos
{
    public sealed class RequestPreviewDto
    {
        public string Template { get; set; }

        public PreviewType TemplateType { get; set; }

        public string AppName { get; set; }
    }
}
