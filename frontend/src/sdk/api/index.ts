/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { SDKConfig } from './../shared';
import { combineUrl, isString } from './../utils';

export interface NotifoNotification {
    // The optional id.
    id?: string;

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

    // The tracking url.
    trackingUrl?: string;

    // True, when silent.
    silent?: boolean;

    // True, when seen.
    isSeen?: boolean;

    // True, when confirmed.
    isConfirmed?: boolean;

    // The timestamp.
    created?: string;
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
        trackingUrl: value.tu,
    };
}

export async function apiMarkConfirmed(notification: NotifoNotification): Promise<any> {
    if (notification.confirmUrl) {
        await fetch(notification.confirmUrl);
    }
}

export async function apiMarkSeen(notification: NotifoNotification): Promise<any> {
    if (notification.trackingUrl) {
        await fetch(notification.trackingUrl);
    }
}

export type ApiResult = { etag: number, notifications: NotifoNotification[] };

export async function apiUpdateSubscription(config: SDKConfig, method: string, topic: string): Promise<boolean> {
    const url = combineUrl(config.apiUrl, `api/web/subscriptions/${topic}`);

    const request: RequestInit = {
        method,
        headers: {
            ...getAuthHeader(config),
        },
    };

    const response = await fetch(url, request);

    if (response.status === 404) {
        return false;
    } else if (response.ok) {
        return true;
    } else {
        throw new Error(`Request failed with {response.statusCode}`);
    }
}

export async function apiUpdateWebPush(config: SDKConfig, params: any, method: string, subscription: PushSubscription) {
    const url = combineUrl(config.apiUrl, 'api/webpush');

    const body: any = {
        subscription,
        ...params,
    };

    const response = await fetch(url, {
        method,
        headers: {
            ...getAuthHeader(config),
            'Content-Type': 'text/json',
        },
        body: JSON.stringify(body),
    });

    if (!response.ok) {
        throw new Error(`Request failed with {response.statusCode}`);
    }
}

export async function apiRegister(config: SDKConfig) {
    if (hasToken(config)) {
        return;
    }

    const CACHE_KEY = `notifo.token-v2`;

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

        if (config.topics.length > 0) {
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
        throw new Error(`Request failed with {response.statusCode}`);
    }

    const result: object = await response.json();

    for (const key in result) {
        if (!result[key]) {
            delete result[key];
        }
    }

    if (config.apiKey) {
        Object.assign(config, result);
    }

    if (window.localStorage) {
        window.localStorage.setItem(CACHE_KEY, JSON.stringify(result));
    }
}

function hasToken(config: SDKConfig) {
    return isString(config.publicKey) && isString(config.userToken);
}

function getAuthHeader(config: SDKConfig) {
    if (config.userToken) {
        return {
            ['X-ApiKey']: config.userToken,
        };
    } else {
        return {
            ['X-ApiKey']: config.apiKey,
        };
    }
}
