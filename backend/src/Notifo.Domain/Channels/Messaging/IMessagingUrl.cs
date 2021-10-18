// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Notifo.Domain.Channels.Messaging
{
    public interface IMessagingUrl
    {
        string MessagingWebhookUrl(string appId, string integrationId, Dictionary<string, string>? query = null);
    }
}
