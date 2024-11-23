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
using Notifo.Domain.Liquid;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates;

[Route("api/apps/{appId:notEmpty}/messaging-templates")]
[ApiExplorerSettings(GroupName = "MessagingTemplates")]
public sealed class MessagingTemplatesController(IChannelTemplateStore<MessagingTemplate> channelTemplateStore, LiquidPropertiesProvider propertiesProvider) : ChannelTemplatesController<MessagingTemplate, MessagingTemplateDto>(channelTemplateStore, propertiesProvider)
{
}
