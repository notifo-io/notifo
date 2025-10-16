// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK.Configuration;

namespace Notifo.SDK;

/// <summary>
/// Default implementation of the <see cref="INotifoClient"/> interface.
/// </summary>
public sealed class NotifoClient : INotifoClient
{
    private readonly Lazy<IAppsClient> apps;
    private readonly Lazy<IConfigsClient> configs;
    private readonly Lazy<IEmailTemplatesClient> emailTemplates;
    private readonly Lazy<IEventsClient> events;
    private readonly Lazy<ILogsClient> logs;
    private readonly Lazy<IMediaClient> media;
    private readonly Lazy<IMobilePushClient> mobilePush;
    private readonly Lazy<IMessagingTemplatesClient> messagingTemplates;
    private readonly Lazy<INotificationsClient> notifications;
    private readonly Lazy<IPingClient> ping;
    private readonly Lazy<ISmsTemplatesClient> smsTemplates;
    private readonly Lazy<ISystemUsersClient> systemUsers;
    private readonly Lazy<ITemplatesClient> templates;
    private readonly Lazy<ITopicsClient> topics;
    private readonly Lazy<IUserClient> user;
    private readonly Lazy<IUsersClient> users;
    private readonly IHttpClientProvider httpClientProvider;

    /// <inheritdoc />
    public IAppsClient Apps => apps.Value;

    /// <inheritdoc />
    public IConfigsClient Configs => configs.Value;

    /// <inheritdoc />
    public IEmailTemplatesClient EmailTemplates => emailTemplates.Value;

    /// <inheritdoc />
    public IEventsClient Events => events.Value;

    /// <inheritdoc />
    public ILogsClient Logs => logs.Value;

    /// <inheritdoc />
    public IMediaClient Media => media.Value;

    /// <inheritdoc />
    public IMessagingTemplatesClient MessagingTemplates => messagingTemplates.Value;

    /// <inheritdoc />
    public IMobilePushClient MobilePush => mobilePush.Value;

    /// <inheritdoc />
    public INotificationsClient Notifications => notifications.Value;

    /// <inheritdoc />
    public IPingClient Ping => ping.Value;

    /// <inheritdoc />
    public ISmsTemplatesClient SmsTemplates => smsTemplates.Value;

    /// <inheritdoc />
    public ISystemUsersClient SystemUsers => systemUsers.Value;

    /// <inheritdoc />
    public ITemplatesClient Templates => templates.Value;

    /// <inheritdoc />
    public ITopicsClient Topics => topics.Value;

    /// <inheritdoc />
    public IUserClient User => user.Value;

    /// <inheritdoc />
    public IUsersClient Users => users.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotifoClient"/> class with the HTTP client and the base URL.
    /// </summary>
    /// <param name="httpClientProvider">The HTTP client. Cannot be null.</param>
    /// <param name="readResponseAsString">True, to read the response as string.</param>
    public NotifoClient(IHttpClientProvider httpClientProvider, bool readResponseAsString)
    {
        apps = new Lazy<IAppsClient>(() =>
        {
            return new AppsClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        configs = new Lazy<IConfigsClient>(() =>
        {
            return new ConfigsClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        emailTemplates = new Lazy<IEmailTemplatesClient>(() =>
        {
            return new EmailTemplatesClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        events = new Lazy<IEventsClient>(() =>
        {
            return new EventsClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        logs = new Lazy<ILogsClient>(() =>
        {
            return new LogsClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        media = new Lazy<IMediaClient>(() =>
        {
            return new MediaClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        mobilePush = new Lazy<IMobilePushClient>(() =>
        {
            return new MobilePushClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        messagingTemplates = new Lazy<IMessagingTemplatesClient>(() =>
        {
            return new MessagingTemplatesClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        notifications = new Lazy<INotificationsClient>(() =>
        {
            return new NotificationsClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        ping = new Lazy<IPingClient>(() =>
        {
            return new PingClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        smsTemplates = new Lazy<ISmsTemplatesClient>(() =>
        {
            return new SmsTemplatesClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        systemUsers = new Lazy<ISystemUsersClient>(() =>
        {
            return new SystemUsersClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        templates = new Lazy<ITemplatesClient>(() =>
        {
            return new TemplatesClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        topics = new Lazy<ITopicsClient>(() =>
        {
            return new TopicsClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        user = new Lazy<IUserClient>(() =>
        {
            return new UserClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        users = new Lazy<IUsersClient>(() =>
        {
            return new UsersClient(httpClientProvider)
            {
                ReadResponseAsString = readResponseAsString
            };
        });

        this.httpClientProvider = httpClientProvider;
    }

    /// <inheritdoc />
    public HttpClient CreateHttpClient()
    {
        return httpClientProvider.Get();
    }

    /// <inheritdoc />
    public void ReturnHttpClient(HttpClient httpClient)
    {
        httpClientProvider.Return(httpClient);
    }
}
