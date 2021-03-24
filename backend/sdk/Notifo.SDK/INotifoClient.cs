// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.SDK
{
    public interface INotifoClient
    {
        IAppsClient Apps { get; }

        IConfigsClient Configs { get; }

        IEventsClient Events { get; }

        ILogsClient Logs { get; }

        IMediaClient Media { get; }

        IMobilePushClient MobilePush { get; }

        INotificationsClient Notifications { get; }

        ITemplatesClient Templates { get; }

        ITopicsClient Topics { get; }

        IUsersClient Users { get; }
    }
}
