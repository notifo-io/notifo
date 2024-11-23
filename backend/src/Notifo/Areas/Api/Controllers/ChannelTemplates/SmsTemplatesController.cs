// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Liquid;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates;

[Route("api/apps/{appId:notEmpty}/sms-templates")]
[ApiExplorerSettings(GroupName = "SmsTemplates")]
public sealed class SmsTemplatesController(IChannelTemplateStore<SmsTemplate> channelTemplateStore, LiquidPropertiesProvider propertiesProvider) : ChannelTemplatesController<SmsTemplate, SmsTemplateDto>(channelTemplateStore, propertiesProvider)
{
}
