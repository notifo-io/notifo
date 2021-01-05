/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { isFunction, logError, logWarn, SDKConfig, SubscribeOptions } from '@sdk/shared';

export module PUSH {
    export async function subscribe(config: SDKConfig, options?: SubscribeOptions) {
        if (!isAvailable()) {
            logError('Service workers are not supported.');
            return;
        }

        if (!await getOrAskPushPermission()) {
            return;
        }

        const serviceWorker = await registerServiceWorker(config, options);

        const simpleConfig = buildConfig(config);

        if (serviceWorker.active) {
            serviceWorker.active.postMessage({ type: 'subscribe', config: simpleConfig });
        }
    }

    export async function unsubscribe(config: SDKConfig) {
        if (!isAvailable()) {
            logError('Service workers are not supported.');
            return;
        }

        const serviceWorker = await navigator.serviceWorker.ready;

        const simpleConfig = buildConfig(config);

        if (serviceWorker.active) {
            serviceWorker.active.postMessage({ type: 'unsubscribe', config: simpleConfig });
        }
    }
}

function buildConfig(config: SDKConfig) {
    const clonedConfig = {};

    for (const key in config) {
        const value = config[key];

        if (!isFunction(value)) {
            clonedConfig[key] = value;
        }
    }

    return clonedConfig;
}

async function getOrAskPushPermission() {
    let status = await getNotificationPermissionState();

    if (status === 'denied') {
        return false;
    }

    if (status !== 'granted') {
        status = await askPermission();
    }

    return status === 'granted';
}

async function getNotificationPermissionState(): Promise<NotificationPermission | PermissionState> {
    if (navigator.permissions) {
        const { state } = await navigator.permissions.query({ name: 'notifications' });

        return state;
    } else {
        return Promise.resolve(Notification.permission);
    }
}

function askPermission(): Promise<NotificationPermission | PermissionState> {
    return new Promise((resolve, reject) => {
        const permissionResult = Notification.requestPermission((result) => {
            resolve(result);
        });

        if (permissionResult) {
            permissionResult.then(resolve, reject);
        }
    });
}

async function registerServiceWorker(config: SDKConfig, options?: SubscribeOptions) {
    if (options && options.existingWorker) {
        logWarn('Using existing service worker.');

        return await navigator.serviceWorker.ready;
    } else {
        const serviceWorker = await navigator.serviceWorker.register(config.serviceWorkerUrl);

        await serviceWorker.update();

        return serviceWorker;
    }
}

function isAvailable() {
    return 'serviceWorker' in navigator && 'PushManager' in window;
}
