// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.ChannelTemplates;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates;

[Route("api/apps/{appId:notEmpty}/messaging-templates")]
[ApiExplorerSettings(GroupName = "MessagingTemplates")]
public sealed class MessagingTemplatesController : ChannelTemplatesController<MessagingTemplate, MessagingTemplateDto>
{
    public MessagingTemplatesController(IChannelTemplateStore<MessagingTemplate> channelTemplateStore)
        : base(channelTemplateStore)
    {
    }
}
