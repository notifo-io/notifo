// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Notifo.Infrastructure.Messaging.Implementation
{
    public interface IMessagingProvider
    {
        IMessaging<T> GetMessaging<T>(IServiceProvider serviceProvider, string channelName);
    }
}
