/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { SWMessage } from '@sdk/push/shared';
import { apiDeleteWebPush, apiPostWebPush, logWarn, NotifoNotification, parseShortNotification, SDKConfig, withPreset } from '@sdk/shared';

(function (self: ServiceWorkerGlobalScope) {
    self.addEventListener('install', () => {
        self.skipWaiting();
    });

    self.addEventListener('message', (event: MessageEvent) => {
        const message: SWMessage = event.data;

        switch (message.type) {
            case 'subscribe': {
                subscribeToWebPush(self.registration, message.config);
                break;
            }
            case 'unsubscribe': {
                unsubscribeFromWebPush(self.registration, message.config);
                break;
            }
        }

    });

    self.addEventListener('notificationclose', (event: NotificationEvent) => {
        const notification: NotifoNotification = event.notification.data;

        if (notification.trackingUrl) {
            const promise = fetch(notification.trackingUrl);

            event.waitUntil(promise);
        }
    });

    self.addEventListener('notificationclick', (event: NotificationEvent) => {
        const notification: NotifoNotification = event.notification.data;

        if (event.action === 'confirm') {
            if (notification.confirmUrl) {
                const promise = fetch(notification.confirmUrl);

                event.waitUntil(promise);
            }
        } else {
            if (notification.linkUrl && self.clients.openWindow) {
                const promise = self.clients.openWindow(notification.linkUrl);

                event.notification.close();
                event.waitUntil(promise);
            }
        }
    });

    self.addEventListener('push', (event: PushEvent) => {
        if (event.data) {
            const notification: NotifoNotification = parseShortNotification(event.data.json());

            const promise = self.registration.getNotifications({ tag: notification.id })
                .then(notifications => {
                    for (const notification of notifications) {
                        notification.close();

                        return Promise.resolve();
                    }

                    const options: NotificationOptions = {
                        data: notification,
                    };

                    options.tag = notification.id;

                    if (notification.confirmUrl && notification.confirmText && !notification.isConfirmed) {
                        options.actions = [{ action: 'confirm', title: notification.confirmText }];
                        options.requireInteraction = true;
                    }

                    if (notification.body) {
                        options.body = notification.body;
                    }

                    if (notification.imageSmall) {
                        options.icon = withPreset(notification.imageSmall, 'WebPushSmall');
                    }

                    if (notification.imageLarge) {
                        options.image = withPreset(notification.imageLarge, 'WebPushSmall');
                    }

                    return self.registration.showNotification(notification.subject, options);
                });

            event.waitUntil(promise);
        }
    });
 })(<any>self);

async function subscribeToWebPush(sw: ServiceWorkerRegistration, config: SDKConfig) {
    const options = {
        userVisibleOnly: true,
        userId: '',
        applicationServerKey: urlB64ToUint8Array(config.publicKey!),
    };

    const subscription = await sw.pushManager.subscribe(options);

    await apiPostWebPush(config, subscription);
}

async function unsubscribeFromWebPush(sw: ServiceWorkerRegistration, config: SDKConfig) {
    const subscription = await sw.pushManager.getSubscription();

    if (!subscription) {
        logWarn('No web push subscription available.');
        return;
    }

    if (!subscription.unsubscribe()) {
        logWarn('Unsubscribe failed.');
        return;
    }

    await apiDeleteWebPush(config, subscription);
}

function urlB64ToUint8Array(base64String: string) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);

    const base64 = (base64String + padding)
        .replace(/\-/g, '+')
        .replace(/_/g, '/');

    const rawData = self.atob(base64);

    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }

    return outputArray;
}
