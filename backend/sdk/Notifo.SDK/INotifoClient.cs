// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.SDK
{
    /// <summary>
    /// Provides access to the individual clients.
    /// </summary>
    public interface INotifoClient
    {
        /// <summary>
        /// Provides the client to query and manage apps.
        /// </summary>
        IAppsClient Apps { get; }

        /// <summary>
        /// Provides the client to query and manage configuration.
        /// </summary>
        IConfigsClient Configs { get; }

        /// <summary>
        /// Provides the client to query and manage events.
        /// </summary>
        IEventsClient Events { get; }

        /// <summary>
        /// Provides the client to query and manage logs.
        /// </summary>
        ILogsClient Logs { get; }

        /// <summary>
        /// Provides the client to query and manage media.
        /// </summary>
        IMediaClient Media { get; }

        /// <summary>
        /// Provides the client to query and manage mobile push settings.
        /// </summary>
        IMobilePushClient MobilePush { get; }

        /// <summary>
        /// Provides the client to query and manage notifications.
        /// </summary>
        INotificationsClient Notifications { get; }

        /// <summary>
        /// Provides the client to query and manage templates.
        /// </summary>
        ITemplatesClient Templates { get; }

        /// <summary>
        /// Provides the client to query and manage topics.
        /// </summary>
        ITopicsClient Topics { get; }

        /// <summary>
        /// Provides the client to query and manage users.
        /// </summary>
        IUsersClient Users { get; }
    }
}
