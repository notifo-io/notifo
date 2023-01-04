// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.SDK;

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
    /// Provides the client to query and manage email templates.
    /// </summary>
    IEmailTemplatesClient EmailTemplates { get; }

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
    /// Provides the client to query and manage messaging templates.
    /// </summary>
    IMessagingTemplatesClient MessagingTemplates { get; }

    /// <summary>
    /// Provides the client to query and manage notifications.
    /// </summary>
    INotificationsClient Notifications { get; }

    /// <summary>
    /// Provides the client to make ping requests.
    /// </summary>
    IPingClient Ping { get; }

    /// <summary>
    /// Provides the client to query and manage SMS templates.
    /// </summary>
    ISmsTemplatesClient SmsTemplates { get; }

    /// <summary>
    /// Provides the client to query and manage system users.
    /// </summary>
    ISystemUsersClient SystemUsers { get; }

    /// <summary>
    /// Provides the client to query and manage templates.
    /// </summary>
    ITemplatesClient Templates { get; }

    /// <summary>
    /// Provides the client to query and manage topics.
    /// </summary>
    ITopicsClient Topics { get; }

    /// <summary>
    /// Provides the client to query user specific endpoints.
    /// </summary>
    IUserClient User { get; }

    /// <summary>
    /// Provides the client to query and manage users.
    /// </summary>
    IUsersClient Users { get; }

    /// <summary>
    /// Creates a <see cref="HttpClient"/> to make all kind of authorized requests.
    /// </summary>
    /// <returns>
    /// The created client.
    /// </returns>
    /// <remarks>
    /// Remember to return the client after each request.
    /// </remarks>
    HttpClient CreateHttpClient();

    /// <summary>
    /// Returns the http client.
    /// </summary>
    /// <param name="httpClient">The HTTP client to return.</param>
    void ReturnHttpClient(HttpClient httpClient);
}
