// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Sms
{
    public interface ISmsUrl
    {
        string SmsWebhookUrl(string appId, string integrationId, Dictionary<string, string>? query = null);
    }
}
