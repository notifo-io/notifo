// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Messaging;

public interface IMessagingFormatter : IChannelTemplateFactory<MessagingTemplate>
{
    string Format(MessagingTemplate? template, BaseUserNotification notification);
}
