// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.ChannelTemplates;
using NSwag.Annotations;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates;

[Route("api/apps/{appId:notEmpty}/email-templates")]
[OpenApiTag("EmailTemplates")]
public sealed class EmailTemplatesController : ChannelTemplatesController<EmailTemplate, EmailTemplateDto>
{
    public EmailTemplatesController(IChannelTemplateStore<EmailTemplate> channelTemplateStore)
        : base(channelTemplateStore)
    {
    }
}
