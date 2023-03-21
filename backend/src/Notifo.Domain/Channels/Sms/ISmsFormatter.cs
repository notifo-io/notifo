// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;

namespace Notifo.Domain.Channels.Sms;

public interface ISmsFormatter : IChannelTemplateFactory<SmsTemplate>
{
    (string Result, List<TemplateError>? Errors) Format(SmsTemplate? template, SmsJob job, App app, User user);
}
