/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

/* eslint-disable no-restricted-globals */

import { SWMessage } from '@sdk/push/shared';
import { apiDeleteWebPush, apiPostWebPush, logWarn, NotifoNotificationDto, parseShortNotification, SDKConfig, withPreset } from '@sdk/shared';

// eslint-disable-next-line func-names
(function (self: ServiceWorkerGlobalScope) {
    self.addEventListener('notificationclick', event => {
        const notification: NotifoNotificationDto = event.notification.data.notification;

        if (notification.trackSeenUrl && !event.notification.data.tracked) {
            event.notification.data.tracked = true;

            const promise = fetch(notification.trackSeenUrl);

            event.waitUntil(promise);
        }

        if (event.action === 'confirm') {
            if (notification.confirmLink && self.clients.openWindow) {
                const promise = self.clients.openWindow(notification.confirmLink);
                
                event.waitUntil(promise);
            }

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

    self.addEventListener('notificationclose', event => {
        const notification: NotifoNotificationDto = event.notification.data.notification;

        if (notification.trackSeenUrl && !event.notification.data.manualClose) {
            const promise = fetch(notification.trackSeenUrl);

            event.waitUntil(promise);
        }
    });

    self.addEventListener('install', () => {
        self.skipWaiting();
    });

    self.addEventListener('message', event => {
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

    self.addEventListener('push', event => {
        if (event.data) {
            const notification = parseShortNotification(event.data.json());

            const promise = showNotification(self, notification);

            event.waitUntil(promise);
        }
    });
// eslint-disable-next-line no-restricted-globals
}(<any>self));

async function subscribeToWebPush(sw: ServiceWorkerRegistration, config: SDKConfig) {
    if (!config.publicKey) {
        return;
    }

    const options = {
        userVisibleOnly: true,
        userId: '',
        applicationServerKey: urlB64ToUint8Array(config.publicKey),
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

async function showNotification(self: ServiceWorkerGlobalScope, notification: NotifoNotificationDto) {
    const query = { tag: notification.id };

    const oldNotifications = await self.registration.getNotifications(query);

    for (const openNotification of oldNotifications) {
        openNotification.data.tracked = true;
        openNotification.close();
    }

    const options: NotificationOptions = {
        data: { notification },
    };

    options.tag = notification.id;

    if (notification.confirmUrl && notification.confirmText && !notification.isConfirmed) {
        options.requireInteraction = true;
        options.actions ||= [];
        options.actions.push({ action: 'confirm', title: notification.confirmText });
    }

    if (notification.body) {
        options.body = notification.body;
    }

    if (notification.imageSmall) {
        options.icon = withPreset(notification.imageSmall, 'WebPushSmall');
    }

    if (notification.imageLarge) {
        options.image = withPreset(notification.imageLarge, 'WebPushLarge');
    }

    await self.registration.showNotification(notification.subject, options);

    if (notification.trackDeliveredUrl) {
        fetch(notification.trackDeliveredUrl);
    }
}

function urlB64ToUint8Array(base64String: string) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);

    const base64 = (base64String + padding)
        // eslint-disable-next-line no-useless-escape
        .replace(/\-/g, '+')
        .replace(/_/g, '/');

    const rawData = self.atob(base64);

    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }

    return outputArray;
}
