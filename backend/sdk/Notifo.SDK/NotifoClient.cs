// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;

namespace Notifo.SDK
{
    public sealed class NotifoClient : INotifoClient
    {
        private readonly Lazy<IAppsClient> apps;
        private readonly Lazy<IConfigsClient> configs;
        private readonly Lazy<IEventsClient> events;
        private readonly Lazy<ILogsClient> logs;
        private readonly Lazy<IMediaClient> media;
        private readonly Lazy<IMobilePushClient> mobilePush;
        private readonly Lazy<INotificationsClient> notifications;
        private readonly Lazy<ITemplatesClient> templates;
        private readonly Lazy<ITopicsClient> topics;
        private readonly Lazy<IUsersClient> users;

        public IAppsClient Apps => apps.Value;

        public IConfigsClient Configs => configs.Value;

        public IEventsClient Events => events.Value;

        public ILogsClient Logs => logs.Value;

        public IMediaClient Media => media.Value;

        public IMobilePushClient MobilePush => mobilePush.Value;

        public INotificationsClient Notifications => notifications.Value;

        public ITemplatesClient Templates => templates.Value;

        public ITopicsClient Topics => topics.Value;

        public IUsersClient Users => users.Value;

        public NotifoClient(HttpClient httpClient, string baseUrl)
        {
            apps = new Lazy<IAppsClient>(() =>
            {
                return new AppsClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            configs = new Lazy<IConfigsClient>(() =>
            {
                return new ConfigsClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            events = new Lazy<IEventsClient>(() =>
            {
                return new EventsClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            logs = new Lazy<ILogsClient>(() =>
            {
                return new LogsClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            media = new Lazy<IMediaClient>(() =>
            {
                return new MediaClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            mobilePush = new Lazy<IMobilePushClient>(() =>
            {
                return new MobilePushClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            notifications = new Lazy<INotificationsClient>(() =>
            {
                return new NotificationsClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            templates = new Lazy<ITemplatesClient>(() =>
            {
                return new TemplatesClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            topics = new Lazy<ITopicsClient>(() =>
            {
                return new TopicsClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            users = new Lazy<IUsersClient>(() =>
            {
                return new UsersClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });
        }
    }
}
