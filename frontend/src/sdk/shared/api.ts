/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/* eslint-disable @typescript-eslint/return-await */

import { ConnectionMode, SDKConfig } from './config';
import { combineUrl, isString } from './utils';

export type NotificationSend = 'Inherit' | 'Send' | 'DoNotSend';
export type NotificationChannel = { send: NotificationSend };
export type NotificationSettings = { [channel: string]: NotificationChannel };
export type TopicChannel = 'Allowed' | 'NotAllowed';

export interface NotifoNotification {
    // The optional id.
    id: string;

    // The notification subject.
    subject: string;

    // The notification body.
    body?: string;

    // The small image
    imageSmall?: string;

    // The large image
    imageLarge?: string;

    // The optional link url.
    linkUrl?: string;

    // The optional link text.
    linkText?: string;

    // The confirm url.
    confirmUrl?: string;

    // The confirm text.
    confirmText?: string;

    // The url to mark the notification as deliverd.
    trackDeliveredUrl?: string;

    // The url to mark the notification as seen.
    trackSeenUrl?: string;

    // True, when silent.
    silent?: boolean;

    // True, when seen.
    isSeen?: boolean;

    // True, when confirmed.
    isConfirmed?: boolean;

    // The timestamp.
    created?: string;

    // The update timestamp.
    updated?: string;
}

export interface ConnectDto {
    // The supported connection mode.
    connectionMode: ConnectionMode;
}

export interface Subscription {
    // The prefix to the topic.
    topicPrefix?: string;

    // The notification settings.
    topicSettings: NotificationSettings;
}

export type Subscriptions = { [path: string]: Subscription | null };

export interface Topic {
    // The path.
    path: string;

    // The required name.
    name: string;

    // The optional description.
    description?: string;

    // The  channel settings.
    channels: { [name: string]: TopicChannel };

    // The notification settings.
    subscription?: NotificationSettings;
}

export interface Profile extends UpdateProfile {
    // The support languages configured in the app.
    supportedLanguages: ReadonlyArray<string>;

    // All available timeones.
    supportedTimezones: ReadonlyArray<string>;
}

export interface UpdateProfile {
    // The email address.
    emailAddress?: string;

    // The full name.
    fullName?: string;

    // The preferred language.
    preferredLanguage: string;

    // The preferred timezone.
    preferredTimezone: string;

    // The notification settings.
    settings?: NotificationSettings;
}

export function parseShortNotification(value: any): NotifoNotification {
    return {
        id: value.id,
        body: value.nb,
        confirmText: value.ct,
        confirmUrl: value.cu,
        imageLarge: value.il,
        imageSmall: value.is,
        isConfirmed: value.ci,
        linkText: value.lt,
        linkUrl: value.lu,
        subject: value.ns,
        trackDeliveredUrl: value.td,
        trackSeenUrl: value.ts,
    };
}

export function booleanToSend(send: boolean | undefined) {
    switch (send) {
        case true:
            return 'Send';
        case false:
            return 'DoNotSend';
        default:
            return 'Inherit';
    }
}

export function sendToBoolean(send: NotificationSend | undefined) {
    switch (send) {
        case 'Send':
            return true;
        case 'DoNotSend':
            return false;
        default:
            return undefined;
    }
}

export async function apiPostSubscription(config: SDKConfig, subscription: Subscription & { topicPrefix: string }): Promise<Subscription | null> {
    const url = combineUrl(config.apiUrl, 'api/me/subscriptions/');

    const response = await fetch(url, {
        method: 'POST',
        headers: {
            ...getAuthHeader(config),
            'Content-Type': 'text/json',
        },
        body: JSON.stringify(subscription),
    });

    if (response.status === 404) {
        return null;
    } else if (response.ok) {
        return await response.json();
    } else {
        throw new Error(`Request failed with ${response.status}`);
    }
}

export async function apiGetConnect(config: SDKConfig): Promise<any> {
    const url = combineUrl(config.apiUrl, 'api/me/web/connect');

    const response = await fetch(url, {
        method: 'POST',
        headers: {
            ...getAuthHeader(config),
            'Content-Type': 'application/json',
        },
    });

    if (!response.ok) {
        throw new Error('Request failed with {response.statusCode}');
    }

    const result = await response.json();

    Object.assign(config, result);
}

export async function apiGetArchive(config: SDKConfig): Promise<ReadonlyArray<NotifoNotification>> {
    const url = combineUrl(config.apiUrl, 'api/me/notifications/archive');

    const response = await fetch(url, {
        method: 'GET',
        headers: {
            ...getAuthHeader(config),
        },
    });

    if (response.status === 404) {
        return [];
    } else if (response.ok) {
        return (await response.json()).items;
    } else {
        throw new Error(`Request failed with ${response.status}`);
    }
}

export async function apiGetTopics(config: SDKConfig): Promise<ReadonlyArray<Topic>> {
    const url = combineUrl(config.apiUrl, 'api/me/topics');

    const response = await fetch(url, {
        method: 'GET',
        headers: {
            ...getAuthHeader(config),
        },
    });

    if (response.status === 404) {
        return [];
    } else if (response.ok) {
        return await response.json();
    } else {
        throw new Error(`Request failed with ${response.status}`);
    }
}

export async function apiGetSubscriptions(config: SDKConfig, topics: string[]): Promise<Subscription[]> {
    const url = combineUrl(config.apiUrl, `api/me/subscriptions/?topics=${topics.join(',')}`);

    const response = await fetch(url, {
        method: 'GET',
        headers: {
            ...getAuthHeader(config),
        },
    });

    if (response.ok) {
        return (await response.json()).items;
    } else {
        throw new Error(`Request failed with ${response.status}`);
    }
}

export async function apiGetProfile(config: SDKConfig): Promise<Profile | null> {
    const url = combineUrl(config.apiUrl, 'api/me/');

    const response = await fetch(url, {
        method: 'GET',
        headers: {
            ...getAuthHeader(config),
        },
    });

    if (response.status === 404) {
        return null;
    } else if (response.ok) {
        return await response.json();
    } else {
        throw new Error(`Request failed with ${response.status}`);
    }
}

export async function apiPostProfile(config: SDKConfig, update: UpdateProfile): Promise<Profile> {
    const url = combineUrl(config.apiUrl, 'api/me/');

    const response = await fetch(url, {
        method: 'POST',
        headers: {
            ...getAuthHeader(config),
            'Content-Type': 'text/json',
        },
        body: JSON.stringify(update),
    });

    if (response.status === 404) {
        throw new Error(`Request failed with ${response.status}`);
    } else if (response.ok) {
        return await response.json();
    } else {
        throw new Error(`Request failed with ${response.status}`);
    }
}

export async function apiDeleteSubscription(config: SDKConfig, topicPrefix: string): Promise<any> {
    const url = combineUrl(config.apiUrl, `api/me/subscriptions/${topicPrefix}`);

    await fetch(url, {
        method: 'DELETE',
        headers: {
            ...getAuthHeader(config),
        },
    });
}

export async function apiPostWebPush(config: SDKConfig, subscription: PushSubscription) {
    const url = combineUrl(config.apiUrl, 'api/me/webpush');

    const response = await fetch(url, {
        method: 'POST',
        headers: {
            ...getAuthHeader(config),
            'Content-Type': 'text/json',
        },
        body: JSON.stringify({ subscription }),
    });

    if (!response.ok) {
        throw new Error(`Request failed with ${response.status}`);
    }
}

export async function apiDeleteWebPush(config: SDKConfig, subscription: PushSubscription) {
    const url = combineUrl(config.apiUrl, 'api/me/webpush');

    const response = await fetch(url, {
        method: 'DELETE',
        headers: {
            ...getAuthHeader(config),
            'Content-Type': 'text/json',
        },
        body: JSON.stringify({ subscription }),
    });

    if (!response.ok) {
        throw new Error(`Request failed with ${response.status}`);
    }
}

export async function apiRegister(config: SDKConfig) {
    if (hasToken(config)) {
        return;
    }

    const CACHE_KEY = 'notifo.token-v2';

    if (window.localStorage) {
        const stored = window.localStorage.getItem(CACHE_KEY);

        if (stored) {
            Object.assign(config, JSON.parse(stored));
        }
    }

    if (hasToken(config)) {
        return;
    }

    const url = combineUrl(config.apiUrl, 'api/web/register');

    const body: any = {};

    if (config.apiKey) {
        if (config.userName) {
            body.emailAddress = config.userEmail;
        }

        if (config.userName) {
            body.displayName = config.userName;
        }

        if (config.userTimezone) {
            body.preferredTimezone = config.userTimezone;
        }

        if (config.userName) {
            body.preferredLanguage = config.userLanguage;
        }

        if (config.topics && config.topics.length > 0) {
            body.topics = config.topics;
        }

        body.createUser = true;
    }

    const response = await fetch(url, {
        method: 'POST',
        headers: {
            ...getAuthHeader(config),
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(body),
    });

    if (!response.ok) {
        throw new Error('Request failed with {response.statusCode}');
    }

    const result: object = await response.json();

    for (const key in result) {
        if (!result[key]) {
            delete result[key];
        }
    }

    Object.assign(config, result);

    if (window.localStorage) {
        window.localStorage.setItem(CACHE_KEY, JSON.stringify(result));
    }
}

function hasToken(config: SDKConfig) {
    return isString(config.publicKey) && isString(config.userToken);
}

function getAuthHeader(config: SDKConfig): Record<string, string> {
    if (config.userToken) {
        return {
            'X-ApiKey': config.userToken,
        };
    } else {
        return {
            'X-ApiKey': config.apiKey!,
        };
    }
}
