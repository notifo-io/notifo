/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { Log, User, UserManager, WebStorageStateStore } from 'oidc-client';
import { AppsClient, ConfigsClient, EmailTemplatesClient, EventsClient, LogsClient, MediaClient, MessagingTemplatesClient, NotificationsClient, SmsTemplatesClient, SystemUsersClient, TemplatesClient, TopicsClient, UsersClient } from './service';

export * from './service';

export function getApiUrl() {
    const baseElements = document.getElementsByTagName('base');

    let baseHref = null;

    if (baseElements.length > 0) {
        baseHref = baseElements[0].href;
    }

    baseHref = baseHref || '';

    let apiUrl: string;

    if (baseHref && baseHref.indexOf(window.location.protocol) === 0) {
        apiUrl = baseHref;
    } else {
        apiUrl = `${window.location.protocol}//${window.location.host}${baseHref}`;
    }

    while (apiUrl.endsWith('/')) {
        apiUrl = apiUrl.substring(0, apiUrl.length - 1);
    }

    return apiUrl;
}

export module AuthService {
    let userManager: UserManager;
    let userCurrent: User | undefined | null;

    export function getCurrentUser() {
        return userCurrent;
    }

    export function getUserManager(): UserManager {
        if (!userManager) {
            const authority = getApiUrl();

            Log.logger = console;

            userManager = new UserManager({
                authority,
                automaticSilentRenew: true,
                client_id: 'notifo',
                client_secret: undefined,
                post_logout_redirect_uri: `${authority}/authentication/logout-callback`,
                redirect_uri: `${authority}/authentication/login-callback`,
                response_type: 'code',
                scope: 'openid profile roles NotifoAPI',
                silentRequestTimeout: 10000,
                silent_redirect_uri: `${authority}/authentication/login-silent-callback.html`,
                userStore: new WebStorageStateStore({ store: window.localStorage || window.sessionStorage }),
            });

            userManager.getUser().then(user => {
                userCurrent = user;
            });

            userManager.events.addUserLoaded(user => {
                userCurrent = user;
            });

            userManager.events.addUserUnloaded(() => {
                userCurrent = undefined;
            });
        }

        return userManager;
    }
}

export module Clients {
    const http = {
        fetch: async (url: RequestInfo, init: RequestInit) => {
            const userManager = AuthService.getUserManager();

            try {
                const user = await userManager.getUser();

                init.headers = init.headers || {};
                init.headers['Authorization'] = `${user?.token_type} ${user?.access_token}`;
            } catch (error) {
                if (init.headers) {
                    delete init.headers['Authorization'];
                }
            }

            let result = await fetch(url, init);

            if (result.status === 401) {
                try {
                    const user = await AuthService.getUserManager().signinSilent();

                    init.headers = init.headers || {};
                    init.headers['Authorization'] = `${user?.token_type} ${user?.access_token}`;
                } catch (error) {
                    if (init.headers) {
                        delete init.headers['Authorization'];
                    }
                }

                result = await fetch(url, init);
            }

            return result;
        },
    };

    export const Apps = new AppsClient(getApiUrl(), http);

    export const Configs = new ConfigsClient(getApiUrl(), http);

    export const EmailTemplates = new EmailTemplatesClient(getApiUrl(), http);

    export const Events = new EventsClient(getApiUrl(), http);

    export const Logs = new LogsClient(getApiUrl(), http);

    export const Media = new MediaClient(getApiUrl(), http);

    export const MessagingTemplates = new MessagingTemplatesClient(getApiUrl(), http);

    export const Notifications = new NotificationsClient(getApiUrl(), http);

    export const SmsTemplates = new SmsTemplatesClient(getApiUrl(), http);

    export const SystemUsers = new SystemUsersClient(getApiUrl(), http);

    export const Topics = new TopicsClient(getApiUrl(), http);

    export const Users = new UsersClient(getApiUrl(), http);

    export const Templates = new TemplatesClient(getApiUrl(), http);
}
