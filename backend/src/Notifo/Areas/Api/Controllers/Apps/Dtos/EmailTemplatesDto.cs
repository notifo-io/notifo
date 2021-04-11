// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Notifo.Domain.Apps;

namespace Notifo.Areas.Api.Controllers.Apps.Dtos
{
    public sealed class EmailTemplatesDto : Dictionary<string, EmailTemplateDto>
    {
        public static EmailTemplatesDto FromDomainObject(App app)
        {
            var result = new EmailTemplatesDto();

            foreach (var (key, value) in app.EmailTemplates)
            {
                if (value != null)
                {
                    result[key] = EmailTemplateDto.FromDomainObject(value);
                }
            }

            return result;
        }
    }
}
