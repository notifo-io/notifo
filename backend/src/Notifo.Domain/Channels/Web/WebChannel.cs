// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.UserNotifications;
using Squidex.Log;

namespace Notifo.Domain.Channels.Web
{
    public sealed class WebChannel : ICommunicationChannel
    {
        private readonly IStreamClient streamClient;
        private readonly ISemanticLog log;

        public string Name => Providers.Web;

        public bool IsSystem => true;

        public WebChannel(IStreamClient streamClient, ISemanticLog log)
        {
            this.streamClient = streamClient;

            this.log = log;
        }

        public IEnumerable<string> GetConfigurations(UserNotification notification, NotificationSetting settings, SendOptions options)
        {
            yield return options.User.Id;
        }

        public async Task SendAsync(UserNotification notification, NotificationSetting settings, string configuration, SendOptions options,
            CancellationToken ct)
        {
            try
            {
                await streamClient.SendAsync(notification);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "SendWeb")
                    .WriteProperty("status", "Failed"));
            }
        }
    }
}
