// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Infrastructure.Messaging.Implementation.GooglePubSub
{
    public sealed class GooglePubSubMessagingProvider : IMessagingProvider
    {
        public IMessaging<T> GetMessaging<T>(IServiceProvider serviceProvider, string channelName)
        {
            return ActivatorUtilities.CreateInstance<GooglePubSubMessaging<T>>(serviceProvider, channelName);
        }
    }
}
