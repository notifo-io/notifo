/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { isDev, isFunction, logError, logWarn, SDKConfig, SubscribeOptions } from '@sdk/shared';

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
        const serviceConfig = buildConfig(config);

        if (serviceWorker.active) {
            serviceWorker.active.postMessage({ type: 'subscribe', config: serviceConfig });
        }
    }

    export async function unsubscribe(config: SDKConfig) {
        if (!isAvailable()) {
            logError('Service workers are not supported.');
            return;
        }

        const serviceWorker = await navigator.serviceWorker.ready;
        const serviceConfig = buildConfig(config);

        if (serviceWorker.active) {
            serviceWorker.active.postMessage({ type: 'unsubscribe', config: serviceConfig });
        }
    }

    export async function isPending() {
        const state = await getNotificationPermissionState();

        return state === 'default' || state === 'prompt';
    }
}

function buildConfig(config: SDKConfig) {
    const clonedConfig: Record<string, any> = {};

    for (const key in config) {
        if (config.hasOwnProperty(key)) {
            const value = (config as any)[key];

            if (!isFunction(value)) {
                clonedConfig[key] = value;
            }
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
        const serviceWorker =
            isDev() ?
                await navigator.serviceWorker.register('/src/sdk/sdk-worker.ts', { type: 'module' }) :
                await navigator.serviceWorker.register(config.serviceWorkerUrl);

        await serviceWorker.update();

        return serviceWorker;
    }
}

function isAvailable() {
    return 'serviceWorker' in navigator && 'PushManager' in window;
}
