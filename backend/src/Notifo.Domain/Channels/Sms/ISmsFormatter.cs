// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.ChannelTemplates;

namespace Notifo.Domain.Channels.Sms
{
    public interface ISmsFormatter : IChannelTemplateFactory<SmsTemplate>
    {
        string Format(SmsTemplate? template, string text);
    }
}
